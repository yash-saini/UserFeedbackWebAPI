using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
    }

    public class AuthenticationService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly PasswordHasher<AppUser> _passwordHasher = new();

        public AuthenticationService(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<string?> LoginAsync(string email, string password)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return "Invalid email or password."; 
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
            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                return "User already exists."; 
            }
            var user = new AppUser
            {
                Email = email,
                PasswordHash = new PasswordHasher<AppUser>().HashPassword(null, password),
                Role = string.IsNullOrEmpty(role) ? "User" : role
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return "User registered successfully."; // Registration successful
        }
    }
}
