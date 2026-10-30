using System.ComponentModel.DataAnnotations;

namespace GLMS.Web.Models
{
    public class ManagerAlert
    {
        public int ManagerAlertId { get; set; }

        public int ContractId { get; set; }
        public Contract? Contract { get; set; }

        [Required, StringLength(300)]
        public string Message { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsDismissed { get; set; }
    }
}