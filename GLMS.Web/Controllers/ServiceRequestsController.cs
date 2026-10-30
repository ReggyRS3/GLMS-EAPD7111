using GLMS.Web.Data;
using GLMS.Web.Models;
using GLMS.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Web.Controllers
{
    public class ServiceRequestsController : Controller
    {
        private readonly GlmsDbContext _context;
        private readonly ServiceRequestService _requestService;
        private readonly ICurrencyConverter _currency;

        public ServiceRequestsController(GlmsDbContext context, ServiceRequestService requestService, ICurrencyConverter currency)
        {
            _context = context;
            _requestService = requestService;
            _currency = currency;
        }

        // GET: ServiceRequests
        public async Task<IActionResult> Index()
        {
            var requests = await _context.ServiceRequests
                .Include(s => s.Contract)
                    .ThenInclude(c => c!.Client)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return View(requests);
        }

        // GET: ServiceRequests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var request = await _context.ServiceRequests
                .Include(s => s.Contract)
                    .ThenInclude(c => c!.Client)
                .FirstOrDefaultAsync(s => s.ServiceRequestId == id);

            if (request == null) return NotFound();

            return View(request);
        }

        // GET: ServiceRequests/Create
        public async Task<IActionResult> Create(int? contractId)
        {
            await LoadContractsAsync(contractId);
            await LoadRateAsync();
            return View(new ServiceRequest { ContractId = contractId ?? 0 });
        }

        // POST: ServiceRequests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ContractId,Description,CostUsd")] ServiceRequest serviceRequest)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var created = await _requestService.CreateRequestAsync(
                        serviceRequest.ContractId, serviceRequest.Description, serviceRequest.CostUsd);

                    TempData["Success"] =
                        $"Service request created: US${created.CostUsd:N2} = R{created.CostZar:N2} (rate {created.ExchangeRate:0.0000}).";

                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            await LoadContractsAsync(serviceRequest.ContractId);
            await LoadRateAsync();
            return View(serviceRequest);
        }

        // GET: ServiceRequests/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var request = await _context.ServiceRequests.FindAsync(id);
            if (request == null) return NotFound();

            return View(request);
        }

        // POST: ServiceRequests/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ServiceRequestId,Description,Status")] ServiceRequest input)
        {
            if (id != input.ServiceRequestId) return NotFound();

            var request = await _context.ServiceRequests.FindAsync(id);
            if (request == null) return NotFound();

            // Only the description and status can be changed after creation
            ModelState.Remove(nameof(ServiceRequest.CostUsd));

            if (!ModelState.IsValid)
            {
                return View(request);
            }

            request.Description = input.Description;
            request.Status = input.Status;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Service request updated.";
            return RedirectToAction(nameof(Index));
        }

        // GET: ServiceRequests/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var request = await _context.ServiceRequests
                .Include(s => s.Contract)
                    .ThenInclude(c => c!.Client)
                .FirstOrDefaultAsync(s => s.ServiceRequestId == id);

            if (request == null) return NotFound();

            return View(request);
        }

        // POST: ServiceRequests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var request = await _context.ServiceRequests.FindAsync(id);
            if (request != null)
            {
                _context.ServiceRequests.Remove(request);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Service request deleted.";
            }

            return RedirectToAction(nameof(Index));
        }

        // Every contract is listed with its status, so the user can see why a request is refused
        private async Task LoadContractsAsync(int? selectedId = null)
        {
            var contracts = await _context.Contracts
                .Include(c => c.Client)
                .OrderBy(c => c.ContractId)
                .ToListAsync();

            ViewData["ContractId"] = new SelectList(
                contracts.Select(c => new
                {
                    c.ContractId,
                    Text = $"#{c.ContractId} - {c.Client?.Name} ({c.Status})"
                }),
                "ContractId", "Text", selectedId);
        }

        // The current rate is sent to the page for the live ZAR preview
        private async Task LoadRateAsync()
        {
            try
            {
                ViewData["UsdToZarRate"] = await _currency.GetUsdToZarRateAsync();
            }
            catch (InvalidOperationException)
            {
                ViewData["UsdToZarRate"] = null;
            }
        }
    }
}