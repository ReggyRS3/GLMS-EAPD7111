using System.ComponentModel.DataAnnotations;

namespace GLMS.Web.Models
{
    public class LoginViewModel
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }

    // What the API returns from POST api/auth/login
    public class LoginResult
    {
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }

    // What the API returns from GET api/dashboard
    public class Dashboard
    {
        public int ActiveCount { get; set; }
        public int OnHoldCount { get; set; }
        public int ExpiredCount { get; set; }
        public int PendingCount { get; set; }
        public List<ManagerAlert> Alerts { get; set; } = new();
    }
}