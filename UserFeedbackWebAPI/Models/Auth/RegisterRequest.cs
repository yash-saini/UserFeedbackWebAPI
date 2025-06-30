﻿namespace UserFeedbackWebAPI.Models.Auth
{
    public class RegisterRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; } = "User";
    }
}
