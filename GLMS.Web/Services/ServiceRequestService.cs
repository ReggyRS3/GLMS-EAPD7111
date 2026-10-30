using GLMS.Web.Data;
using GLMS.Web.Models;

namespace GLMS.Web.Services
{
    public class ServiceRequestService
    {
        private readonly GlmsDbContext _context;
        private readonly ICurrencyConverter _converter;

        public ServiceRequestService(GlmsDbContext context, ICurrencyConverter converter)
        {
            _context = context;
            _converter = converter;
        }

        public async Task<ServiceRequest> CreateRequestAsync(int contractId, string description, decimal costUsd)
        {
            var contract = await _context.Contracts.FindAsync(contractId)
                ?? throw new InvalidOperationException("Please select a valid contract.");

            // Workflow rule: only Active contracts accept requests (decided by the State pattern)
            if (!contract.CanRaiseServiceRequest())
            {
                throw new InvalidOperationException(
                    $"A service request cannot be raised against a contract that is {contract.Status}. Only Active contracts accept new requests.");
            }

            var rate = await _converter.GetUsdToZarRateAsync();

            var request = new ServiceRequest
            {
                ContractId = contractId,
                Description = description,
                CostUsd = costUsd,
                ExchangeRate = rate,
                CostZar = _converter.ConvertUsdToZar(costUsd, rate),
                Status = ServiceRequestStatus.Pending,
                CreatedAt = DateTime.Now
            };

            _context.ServiceRequests.Add(request);
            await _context.SaveChangesAsync();

            return request;
        }
    }
}