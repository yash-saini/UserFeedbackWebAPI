using System.ComponentModel.DataAnnotations;

namespace UserFeedbackWebAPI.Models
{
    public class AppUser
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "User"; // Default role
    }
}
