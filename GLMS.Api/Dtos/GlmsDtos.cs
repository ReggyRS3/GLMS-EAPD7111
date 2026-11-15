using System.ComponentModel.DataAnnotations;
using GLMS.Api.Models;

namespace GLMS.Api.Dtos
{
    // ---------- Clients ----------
    public class ClientDto
    {
        public int ClientId { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string ContactEmail { get; set; } = string.Empty;

        [Phone]
        public string? ContactPhone { get; set; }

        [Required, StringLength(50)]
        public string Region { get; set; } = string.Empty;
    }

    // ---------- Contracts ----------
    public class ContractDto
    {
        public int ContractId { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ContractStatus Status { get; set; }
        public string ServiceLevel { get; set; } = string.Empty;
        public bool HasSignedAgreement { get; set; }
        public bool CanRaiseServiceRequest { get; set; }
    }

    public class ContractSaveDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Please select a client.")]
        public int ClientId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required, StringLength(50)]
        public string ServiceLevel { get; set; } = string.Empty;
    }

    public enum ContractAction
    {
        Activate,
        PutOnHold,
        Expire
    }

    public class ContractStatusUpdateDto
    {
        [Required]
        public ContractAction? Action { get; set; }
    }

    public class ContractHistoryDto
    {
        public DateTime ChangedAt { get; set; }
        public string OldStatus { get; set; } = string.Empty;
        public string NewStatus { get; set; } = string.Empty;
    }

    // ---------- Service requests ----------
    public class ServiceRequestDto
    {
        public int ServiceRequestId { get; set; }
        public int ContractId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal CostUsd { get; set; }
        public decimal ExchangeRate { get; set; }
        public decimal CostZar { get; set; }
        public ServiceRequestStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ServiceRequestCreateDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Please select a contract.")]
        public int ContractId { get; set; }

        [Required, StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.01", "10000000", ErrorMessage = "Cost must be between 0.01 and 10,000,000 USD.")]
        public decimal CostUsd { get; set; }
    }

    public class ServiceRequestUpdateDto
    {
        [Required, StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public ServiceRequestStatus? Status { get; set; }
    }

    // ---------- Dashboard and currency ----------
    public class ManagerAlertDto
    {
        public int ManagerAlertId { get; set; }
        public int ContractId { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class DashboardDto
    {
        public int ActiveCount { get; set; }
        public int OnHoldCount { get; set; }
        public int ExpiredCount { get; set; }
        public int PendingCount { get; set; }
        public List<ManagerAlertDto> Alerts { get; set; } = new();
    }

    public class ExchangeRateDto
    {
        public string From { get; set; } = "USD";
        public string To { get; set; } = "ZAR";
        public decimal Rate { get; set; }
    }

    // ---------- Model -> DTO conversion ----------
    public static class DtoMappings
    {
        public static ClientDto ToDto(this Client c) => new()
        {
            ClientId = c.ClientId,
            Name = c.Name,
            ContactEmail = c.ContactEmail,
            ContactPhone = c.ContactPhone,
            Region = c.Region
        };

        public static ContractDto ToDto(this Contract c) => new()
        {
            ContractId = c.ContractId,
            ClientId = c.ClientId,
            ClientName = c.Client?.Name ?? string.Empty,
            StartDate = c.StartDate,
            EndDate = c.EndDate,
            Status = c.Status,
            ServiceLevel = c.ServiceLevel,
            HasSignedAgreement = !string.IsNullOrEmpty(c.SignedAgreementPath),
            CanRaiseServiceRequest = c.CanRaiseServiceRequest()
        };

        public static ServiceRequestDto ToDto(this ServiceRequest s) => new()
        {
            ServiceRequestId = s.ServiceRequestId,
            ContractId = s.ContractId,
            ClientName = s.Contract?.Client?.Name ?? string.Empty,
            Description = s.Description,
            CostUsd = s.CostUsd,
            ExchangeRate = s.ExchangeRate,
            CostZar = s.CostZar,
            Status = s.Status,
            CreatedAt = s.CreatedAt
        };
    }
}