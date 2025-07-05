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
            if (result == "Invalid email format.")
                return BadRequest("Invalid email format.");
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

        [HttpGet("confirm")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string email, [FromQuery] string token)
        {
            var result = await _authService.ConfirmEmailAsync(email, token);

            if (result == "Invalid token or email.")
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("resend-confirmation")]
        public async Task<IActionResult> ResendConfirmation([FromBody] ResendConfirmationRequest request)
        {
            var result = await _authService.ResendConfirmationEmailAsync(request.Email);

            return result switch
            {
                "Already confirmed" => BadRequest("Email is already confirmed."),
                "Not found" => NotFound("Email not registered."),
                "Sent" => Ok("Confirmation email resent successfully."),
                _ => StatusCode(500, "An unexpected error occurred.")
            };
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var result = await _authService.RefreshTokenAsync(request.Email, request.RefreshToken);

            if (result == null)
                return Unauthorized("Invalid or expired refresh token.");

            return Ok(result);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            var success = await _authService.LogoutAsync(request.Email);
            return success ? Ok("Logged out successfully.") : NotFound("User not found.");
        }
    }
}