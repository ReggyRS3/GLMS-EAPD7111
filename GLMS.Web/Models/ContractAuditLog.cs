using System.ComponentModel.DataAnnotations;

namespace GLMS.Web.Models
{
    public class ContractAuditLog
    {
        public int ContractAuditLogId { get; set; }

        public int ContractId { get; set; }
        public Contract? Contract { get; set; }

        [Required, StringLength(20), Display(Name = "Old Status")]
        public string OldStatus { get; set; } = string.Empty;

        [Required, StringLength(20), Display(Name = "New Status")]
        public string NewStatus { get; set; } = string.Empty;

        [Display(Name = "Changed")]
        public DateTime ChangedAt { get; set; } = DateTime.Now;
    }
}