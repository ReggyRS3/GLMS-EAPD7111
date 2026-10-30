using GLMS.Web.Data;
using GLMS.Web.Models;

namespace GLMS.Web.Patterns.Observer
{
    public class ManagerAlertObserver : IContractObserver
    {
        private readonly GlmsDbContext _context;

        public ManagerAlertObserver(GlmsDbContext context)
        {
            _context = context;
        }

        public void Update(Contract contract, string oldStatus)
        {
            // Managers only need an alert when a contract stops accepting service requests
            if (contract.Status is not (ContractStatus.OnHold or ContractStatus.Expired))
            {
                return;
            }

            _context.ManagerAlerts.Add(new ManagerAlert
            {
                ContractId = contract.ContractId,
                Message = $"Contract #{contract.ContractId} changed from {oldStatus} to {contract.Status}. New service requests are now blocked.",
                CreatedAt = DateTime.Now
            });
        }
    }
}