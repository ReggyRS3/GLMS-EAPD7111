using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;

namespace GLMS.Web.Models
{
    public class Client
    {
        public int ClientId { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress, Display(Name = "Contact Email")]
        public string ContactEmail { get; set; } = string.Empty;

        [Phone, Display(Name = "Contact Phone")]
        public string? ContactPhone { get; set; }

        [Required, StringLength(50)]
        public string Region { get; set; } = string.Empty;

        public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    }
}