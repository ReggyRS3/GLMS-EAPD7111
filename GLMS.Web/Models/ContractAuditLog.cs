namespace GLMS.Web.Models
{
    public class ContractAuditLog
    {
        public DateTime ChangedAt { get; set; }
        public string OldStatus { get; set; } = string.Empty;
        public string NewStatus { get; set; } = string.Empty;
    }
}