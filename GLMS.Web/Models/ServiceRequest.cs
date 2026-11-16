using System.ComponentModel.DataAnnotations;

namespace GLMS.Web.Models
{
    public class ServiceRequest
    {
        public int ServiceRequestId { get; set; }

        [Display(Name = "Contract")]
        public int ContractId { get; set; }

        [Display(Name = "Client")]
        public string? ClientName { get; set; }

        [Required, StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Range(0.01, 10000000), Display(Name = "Cost (USD)")]
        public decimal CostUsd { get; set; }

        [Display(Name = "Exchange Rate")]
        public decimal ExchangeRate { get; set; }

        [Display(Name = "Cost (ZAR)")]
        public decimal CostZar { get; set; }

        public ServiceRequestStatus Status { get; set; }

        [Display(Name = "Created")]
        public DateTime CreatedAt { get; set; }
    }
}