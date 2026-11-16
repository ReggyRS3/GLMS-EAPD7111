namespace GLMS.Web.Models
{
    public class ManagerAlert
    {
        public int ManagerAlertId { get; set; }
        public int ContractId { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}