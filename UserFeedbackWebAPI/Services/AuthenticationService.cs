using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserFeedbackWebAPI.Data;
using UserFeedbackWebAPI.Models;

namespace UserFeedbackWebAPI.Services
{
    public interface IAuthService
    {
        Task<string?> RegisterAsync(string email, string password, string role);
        Task<string?> LoginAsync(string email, string password);
        Task<string> ConfirmEmailAsync(string email, string token);
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

        public async Task<string?> LoginAsync(string email, string password)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return "Invalid email or password."; 
            }
            if (!user.IsEmailConfirmed)
            {
                return "Email not confirmed.";
            }
            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            if (result != PasswordVerificationResult.Success)
            {
                return "Invalid email or password.";
            }
            // Generate JWT token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? "this is my custom super secret key for jwt");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
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
            var user = new AppUser
            {
                Email = normalizedEmail,
                PasswordHash = new PasswordHasher<AppUser>().HashPassword(null, password),
                Role = string.IsNullOrEmpty(role) ? "User" : role,
                EmailConfirmationToken = token,
                IsEmailConfirmed = false
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
            user.IsEmailConfirmed = true;
            user.EmailConfirmationToken = null;
            await _context.SaveChangesAsync();

            return "Email confirmed successfully.";
        }

    }
}
