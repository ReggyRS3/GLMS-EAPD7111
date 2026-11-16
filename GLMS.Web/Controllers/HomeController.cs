using System.Diagnostics;
using GLMS.Web.Models;
using GLMS.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GLMS.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly GlmsApiClient _api;

        public HomeController(GlmsApiClient api)
        {
            _api = api;
        }

        public async Task<IActionResult> Index()
        {
            var dashboard = await _api.GetDashboardAsync();

            ViewData["ActiveCount"] = dashboard.ActiveCount;
            ViewData["OnHoldCount"] = dashboard.OnHoldCount;
            ViewData["ExpiredCount"] = dashboard.ExpiredCount;
            ViewData["PendingCount"] = dashboard.PendingCount;

            return View(dashboard.Alerts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DismissAlert(int id)
        {
            await _api.DismissAlertAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}