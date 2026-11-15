using GLMS.Api.Data;
using GLMS.Api.Dtos;
using GLMS.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly GlmsDbContext _context;

        public DashboardController(GlmsDbContext context)
        {
            _context = context;
        }

        // GET: api/dashboard
        [HttpGet]
        public async Task<ActionResult<DashboardDto>> Get()
        {
            var dashboard = new DashboardDto
            {
                ActiveCount = await _context.Contracts.CountAsync(c => c.Status == ContractStatus.Active),
                OnHoldCount = await _context.Contracts.CountAsync(c => c.Status == ContractStatus.OnHold),
                ExpiredCount = await _context.Contracts.CountAsync(c => c.Status == ContractStatus.Expired),
                PendingCount = await _context.ServiceRequests.CountAsync(s => s.Status == ServiceRequestStatus.Pending),
                Alerts = await _context.ManagerAlerts
                    .Where(a => !a.IsDismissed)
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(10)
                    .Select(a => new ManagerAlertDto
                    {
                        ManagerAlertId = a.ManagerAlertId,
                        ContractId = a.ContractId,
                        Message = a.Message,
                        CreatedAt = a.CreatedAt
                    })
                    .ToListAsync()
            };

            return Ok(dashboard);
        }

        // POST: api/dashboard/alerts/5/dismiss
        [HttpPost("alerts/{id:int}/dismiss")]
        public async Task<IActionResult> DismissAlert(int id)
        {
            var alert = await _context.ManagerAlerts.FindAsync(id);
            if (alert == null) return NotFound(new { message = "Alert not found." });

            alert.IsDismissed = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}