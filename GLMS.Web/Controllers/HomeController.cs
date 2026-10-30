using System.Diagnostics;
using GLMS.Web.Data;
using GLMS.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly GlmsDbContext _context;

        public HomeController(GlmsDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["ActiveCount"] = await _context.Contracts.CountAsync(c => c.Status == ContractStatus.Active);
            ViewData["OnHoldCount"] = await _context.Contracts.CountAsync(c => c.Status == ContractStatus.OnHold);
            ViewData["ExpiredCount"] = await _context.Contracts.CountAsync(c => c.Status == ContractStatus.Expired);
            ViewData["PendingCount"] = await _context.ServiceRequests.CountAsync(s => s.Status == ServiceRequestStatus.Pending);

            var alerts = await _context.ManagerAlerts
                .Where(a => !a.IsDismissed)
                .OrderByDescending(a => a.CreatedAt)
                .Take(10)
                .ToListAsync();

            return View(alerts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DismissAlert(int id)
        {
            var alert = await _context.ManagerAlerts.FindAsync(id);
            if (alert != null)
            {
                alert.IsDismissed = true;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}