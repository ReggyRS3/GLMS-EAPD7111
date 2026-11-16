using GLMS.Web.Models;
using GLMS.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GLMS.Web.Controllers
{
    public class ServiceRequestsController : Controller
    {
        private readonly GlmsApiClient _api;

        public ServiceRequestsController(GlmsApiClient api)
        {
            _api = api;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _api.GetServiceRequestsAsync());
        }

        public async Task<IActionResult> Details(int id)
        {
            var request = await _api.GetServiceRequestAsync(id);
            return request == null ? NotFound() : View(request);
        }

        public async Task<IActionResult> Create(int? contractId)
        {
            await LoadContractsAsync(contractId);
            ViewData["UsdToZarRate"] = await _api.GetUsdToZarRateAsync();
            return View(new ServiceRequest { ContractId = contractId ?? 0 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ContractId,Description,CostUsd")] ServiceRequest serviceRequest)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // The API checks the contract is Active and calculates the ZAR cost
                    var created = await _api.CreateServiceRequestAsync(serviceRequest);
                    TempData["Success"] =
                        $"Service request created: US${created.CostUsd:N2} = R{created.CostZar:N2} (rate {created.ExchangeRate:0.0000}).";
                    return RedirectToAction(nameof(Index));
                }
                catch (ApiException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            await LoadContractsAsync(serviceRequest.ContractId);
            ViewData["UsdToZarRate"] = await _api.GetUsdToZarRateAsync();
            return View(serviceRequest);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var request = await _api.GetServiceRequestAsync(id);
            return request == null ? NotFound() : View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ServiceRequestId,Description,Status")] ServiceRequest input)
        {
            if (id != input.ServiceRequestId) return NotFound();

            // Costs are fixed once saved, so they are not part of this form
            ModelState.Remove(nameof(ServiceRequest.CostUsd));

            if (ModelState.IsValid)
            {
                try
                {
                    await _api.UpdateServiceRequestAsync(input);
                    TempData["Success"] = "Service request updated.";
                    return RedirectToAction(nameof(Index));
                }
                catch (ApiException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            var request = await _api.GetServiceRequestAsync(id);
            return request == null ? NotFound() : View(request);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var request = await _api.GetServiceRequestAsync(id);
            return request == null ? NotFound() : View(request);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _api.DeleteServiceRequestAsync(id);
                TempData["Success"] = "Service request deleted.";
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // Every contract is listed with its status, so the user can see why a request is refused
        private async Task LoadContractsAsync(int? selectedId = null)
        {
            var contracts = await _api.GetContractsAsync(null, null, null);

            ViewData["ContractId"] = new SelectList(
                contracts.Select(c => new
                {
                    c.ContractId,
                    Text = $"#{c.ContractId} - {c.ClientName} ({c.Status})"
                }),
                "ContractId", "Text", selectedId);
        }
    }
}