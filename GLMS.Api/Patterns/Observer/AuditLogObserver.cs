using GLMS.Api.Data;
using GLMS.Api.Models;

namespace GLMS.Api.Patterns.Observer
{
    public class AuditLogObserver : IContractObserver
    {
        private readonly GlmsDbContext _context;

        public AuditLogObserver(GlmsDbContext context)
        {
            _context = context;
        }

        public void Update(Contract contract, string oldStatus)
        {
            // Saved together with the status change when the controller calls SaveChangesAsync
            _context.ContractAuditLogs.Add(new ContractAuditLog
            {
                ContractId = contract.ContractId,
                OldStatus = oldStatus,
                NewStatus = contract.Status.ToString(),
                ChangedAt = DateTime.Now
            });
        }
    }
}