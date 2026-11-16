using GLMS.Web.Models;
using GLMS.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GLMS.Web.Controllers
{
    public class ContractsController : Controller
    {
        private readonly GlmsApiClient _api;

        public ContractsController(GlmsApiClient api)
        {
            _api = api;
        }

        // GET: Contracts (search by date range and status; the LINQ filtering runs in the API)
        public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate, ContractStatus? status)
        {
            if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
            {
                ViewData["FilterError"] = "The 'From' date must be before the 'To' date.";
                fromDate = null;
                toDate = null;
            }

            var contracts = await _api.GetContractsAsync(fromDate, toDate, status);

            ViewData["FromDate"] = fromDate?.ToString("yyyy-MM-dd");
            ViewData["ToDate"] = toDate?.ToString("yyyy-MM-dd");
            ViewData["Status"] = status;

            return View(contracts);
        }

        public async Task<IActionResult> Details(int id)
        {
            var contract = await _api.GetContractAsync(id);
            if (contract == null) return NotFound();

            ViewData["History"] = await _api.GetContractHistoryAsync(id);
            return View(contract);
        }

        public async Task<IActionResult> Create()
        {
            await LoadClientsAsync();
            return View(new Contract { StartDate = DateTime.Today, EndDate = DateTime.Today.AddYears(1) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ClientId,StartDate,EndDate,ServiceLevel")] Contract contract)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var created = await _api.CreateContractAsync(contract);
                    TempData["Success"] = "Contract created as Draft. Activate it when it is ready.";
                    return RedirectToAction(nameof(Details), new { id = created.ContractId });
                }
                catch (ApiException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            await LoadClientsAsync(contract.ClientId);
            return View(contract);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var contract = await _api.GetContractAsync(id);
            if (contract == null) return NotFound();

            await LoadClientsAsync(contract.ClientId);
            return View(contract);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ContractId,ClientId,StartDate,EndDate,ServiceLevel")] Contract contract)
        {
            if (id != contract.ContractId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    await _api.UpdateContractAsync(contract);
                    TempData["Success"] = "Contract updated.";
                    return RedirectToAction(nameof(Details), new { id });
                }
                catch (ApiException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            await LoadClientsAsync(contract.ClientId);
            return View(contract);
        }

        // POST: Contracts/ChangeStatus/5 (the API's State pattern decides if the change is allowed)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int id, string command)
        {
            var action = command switch
            {
                "activate" => "Activate",
                "hold" => "PutOnHold",
                "expire" => "Expire",
                _ => null
            };

            if (action == null) return BadRequest();

            try
            {
                var contract = await _api.ChangeContractStatusAsync(id, action);
                TempData["Success"] = $"Contract status changed to {contract.Status}.";
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadAgreement(int id, IFormFile? agreementFile)
        {
            if (agreementFile == null)
            {
                TempData["Error"] = "Please choose a PDF file to upload.";
                return RedirectToAction(nameof(Details), new { id });
            }

            try
            {
                await _api.UploadAgreementAsync(id, agreementFile);
                TempData["Success"] = "Signed agreement uploaded.";
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        public async Task<IActionResult> DownloadAgreement(int id)
        {
            try
            {
                var bytes = await _api.DownloadAgreementAsync(id);
                return File(bytes, "application/pdf", $"Contract-{id}-Signed-Agreement.pdf");
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            var contract = await _api.GetContractAsync(id);
            return contract == null ? NotFound() : View(contract);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _api.DeleteContractAsync(id);
                TempData["Success"] = "Contract deleted.";
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadClientsAsync(int? selectedId = null)
        {
            var clients = await _api.GetClientsAsync();
            ViewData["ClientId"] = new SelectList(clients, "ClientId", "Name", selectedId);
        }
    }
}