using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Web.Models
{
    public class ServiceRequest
    {
        public int ServiceRequestId { get; set; }

        [Display(Name = "Contract")]
        public int ContractId { get; set; }
        public Contract? Contract { get; set; }

        [Required, StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Precision(18, 2), Range(0.01, 10000000), Display(Name = "Cost (USD)")]
        public decimal CostUsd { get; set; }

        [Precision(18, 6), Display(Name = "Exchange Rate")]
        public decimal ExchangeRate { get; set; }

        [Precision(18, 2), Display(Name = "Cost (ZAR)")]
        public decimal CostZar { get; set; }

        public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.Pending;

        [Display(Name = "Created")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}