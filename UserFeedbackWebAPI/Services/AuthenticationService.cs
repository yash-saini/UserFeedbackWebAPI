using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using UserFeedbackWebAPI.Data;
using UserFeedbackWebAPI.Models;
using UserFeedbackWebAPI.Models.Auth;

namespace UserFeedbackWebAPI.Services
{
    public interface IAuthService
    {
        Task<string?> RegisterAsync(string email, string password, string role);
        Task<AuthResponse?> LoginAsync(string email, string password);
        Task<string> ConfirmEmailAsync(string email, string token);
        Task<string> ResendConfirmationEmailAsync(string email);
        Task<AuthResponse?> RefreshTokenAsync(string email, string refreshToken);
    }

    public class AuthenticationService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly PasswordHasher<AppUser> _passwordHasher = new();
        private readonly IEmailService _emailService;

        public AuthenticationService(AppDbContext context, IConfiguration config, IEmailService emailService)
        {
            _context = context;
            _config = config;
            _emailService = emailService;
        }

        public async Task<AuthResponse?> LoginAsync(string email, string password)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
            if (user == null)
                return null;

            if (!user.IsEmailConfirmed)
                return null; // You can return a custom message if needed

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            if (result != PasswordVerificationResult.Success)
                return null;

            // Generate Access Token (short-lived)
            var accessToken = GenerateJwtToken(user);

            // Generate Refresh Token (long-lived)
            var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7); // or 30 days

            Console.WriteLine("Before save: " + user.RefreshToken);
            await _context.SaveChangesAsync();
            Console.WriteLine("After save");

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        public async Task<AuthResponse?> RefreshTokenAsync(string email, string refreshToken)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

            if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiry < DateTime.UtcNow)
                return null;

            // Create new tokens
            var newAccessToken = GenerateJwtToken(user);
            var newRefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
        }

        private string GenerateJwtToken(AppUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        }),
                Expires = DateTime.UtcNow.AddMinutes(15), // Access token expires quickly
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }


        public async Task<string?> RegisterAsync(string email, string password, string role)
        {
            var emailAttribute = new EmailAddressAttribute();
            if (!emailAttribute.IsValid(email))
                return "Invalid email format.";

            var normalizedEmail = email.Trim().ToLower();
            if (await _context.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail))
            {
                return "User already exists."; 
            }
            var token = Guid.NewGuid().ToString();
            var expiry = DateTime.UtcNow.AddHours(24);
            var user = new AppUser
            {
                Email = normalizedEmail,
                PasswordHash = new PasswordHasher<AppUser>().HashPassword(null, password),
                Role = string.IsNullOrEmpty(role) ? "User" : role,
                EmailConfirmationToken = token,
                IsEmailConfirmed = false,
                EmailConfirmationTokenExpires = expiry
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var confirmationLink = $"https://localhost:7071/api/auth/confirm?email={normalizedEmail}&token={token}";
            var subject = "Confirm your email";
            var htmlBody = $"<p>Please confirm your email by clicking <a href='{confirmationLink}'>here</a>.</p>";

            await _emailService.SendEmailAsync(normalizedEmail, subject, htmlBody);

            return "User registered successfully.";
        }

        public async Task<string> ConfirmEmailAsync(string email, string token)
        {
            var normalizedEmail = Uri.UnescapeDataString(email).Trim().ToLower();
            var user = await _context.Users
                .SingleOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);
            if (user == null || user.EmailConfirmationToken != token)
                return "Invalid token or email.";
            if (user.EmailConfirmationTokenExpires < DateTime.UtcNow)
                return "Token has expired. Please request a new confirmation email.";

            user.IsEmailConfirmed = true;
            user.EmailConfirmationToken = null;
            user.EmailConfirmationTokenExpires = null;

            await _context.SaveChangesAsync();
            return "Email confirmed successfully.";
        }

        public async Task<string> ResendConfirmationEmailAsync(string email)
        {
            var normalizedEmail = email.Trim().ToLower();
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);

            if (user == null)
                return "Not found";

            if (user.IsEmailConfirmed)
                return "Already confirmed";

            // Generate a new token
            var token = Guid.NewGuid().ToString();
            user.EmailConfirmationToken = token;
            user.EmailConfirmationTokenExpires = DateTime.UtcNow.AddHours(24);
            await _context.SaveChangesAsync();

            var confirmationLink = $"https://localhost:7071/api/auth/confirm?email={normalizedEmail}&token={token}";
            var subject = "Resend: Confirm your email";
            var htmlBody = $@"
                            <p>Hi again!</p>
                            <p>Please <a href='{confirmationLink}'>click here to confirm your email</a>.</p>";

            await _emailService.SendEmailAsync(user.Email, subject, htmlBody);

            return "Sent";
        }
    }
}
