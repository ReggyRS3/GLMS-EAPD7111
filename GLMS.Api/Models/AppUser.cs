using System.ComponentModel.DataAnnotations;

namespace GLMS.Api.Models
{
    public class AppUser
    {
        public int AppUserId { get; set; }

        [Required, StringLength(50)]
        public string Username { get; set; } = string.Empty;

        // Only a hash is stored, never the real password
        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string Role { get; set; } = "Admin";
    }
}