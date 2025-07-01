using Microsoft.AspNetCore.Mvc;
using UserFeedbackWebAPI.Models.Auth;
using UserFeedbackWebAPI.Services;

namespace UserFeedbackWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest("Email and password are required.");
            }
            if (request.Role != "User" && request.Role != "Admin")
            {
                return BadRequest("Invalid role. Only 'User' or 'Admin' are allowed.");
            }
            var result = await _authService.RegisterAsync(request.Email, request.Password, request.Role);
            if (result == null)
            {
                return BadRequest("Registration failed. User may already exist.");
            }
            return Ok(new { message = "User registered successfully." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest("Email and password are required.");
            }
            var token = await _authService.LoginAsync(request.Email, request.Password);
            if (token == null)
            {
                return Unauthorized("Invalid email or password.");
            }
            return Ok(new { token });
        }
    }
}