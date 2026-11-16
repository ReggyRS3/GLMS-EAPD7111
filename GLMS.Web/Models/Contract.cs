using System.ComponentModel.DataAnnotations;

namespace GLMS.Web.Models
{
    // What the API sends for a contract; the rules (State pattern) now live in GLMS.Api
    public class Contract
    {
        public int ContractId { get; set; }

        [Display(Name = "Client")]
        public int ClientId { get; set; }

        [Display(Name = "Client")]
        public string? ClientName { get; set; }

        [DataType(DataType.Date), Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date), Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        public ContractStatus Status { get; set; }

        [Required, StringLength(50), Display(Name = "Service Level")]
        public string ServiceLevel { get; set; } = string.Empty;

        [Display(Name = "Signed Agreement")]
        public bool HasSignedAgreement { get; set; }

        [Display(Name = "Service requests allowed")]
        public bool CanRaiseServiceRequest { get; set; }
    }
}