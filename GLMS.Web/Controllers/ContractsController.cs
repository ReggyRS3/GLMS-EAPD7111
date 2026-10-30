using GLMS.Web.Data;
using GLMS.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GLMS.Web.Services;
using GLMS.Web.Patterns.Observer;

namespace GLMS.Web.Controllers
{
    public class ContractsController : Controller
    {
        private readonly GlmsDbContext _context;
        private readonly IFileStorageService _files;

        public ContractsController(GlmsDbContext context, IFileStorageService files)
        {
            _context = context;
            _files = files;
        }

        // GET: Contracts
        public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate, ContractStatus? status)
        {
            // Automated status tracking: active or on-hold contracts past their end date are expired
            var overdue = await _context.Contracts
                .Where(c => c.EndDate < DateTime.Today &&
                           (c.Status == ContractStatus.Active || c.Status == ContractStatus.OnHold))
                .ToListAsync();

            foreach (var contract in overdue)
            {
                AttachObservers(contract);
                contract.Expire();
            }

            if (overdue.Any())
            {
                await _context.SaveChangesAsync();
            }

            if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
            {
                ViewData["FilterError"] = "The 'From' date must be before the 'To' date.";
                fromDate = null;
                toDate = null;
            }

            IQueryable<Contract> query = _context.Contracts.Include(c => c.Client);
            query = ContractFilter.Apply(query, fromDate, toDate, status);

            var contracts = await query
                .OrderBy(c => c.EndDate)
                .ToListAsync();

            // Keep the search boxes filled in after searching
            ViewData["FromDate"] = fromDate?.ToString("yyyy-MM-dd");
            ViewData["ToDate"] = toDate?.ToString("yyyy-MM-dd");
            ViewData["Status"] = status;

            return View(contracts);
        }

        // GET: Contracts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var contract = await _context.Contracts
                .Include(c => c.Client)
                .Include(c => c.ServiceRequests)
                .FirstOrDefaultAsync(c => c.ContractId == id);

            if (contract == null) return NotFound();

            ViewData["History"] = await _context.ContractAuditLogs
                .Where(l => l.ContractId == contract.ContractId)
                .OrderByDescending(l => l.ChangedAt)
                .ToListAsync();

            return View(contract);
        }

        // GET: Contracts/Create
        public IActionResult Create()
        {
            LoadClients();
            return View(new Contract
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddYears(1)
            });

           
        }

        // POST: Contracts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ClientId,StartDate,EndDate,ServiceLevel")] Contract contract)
        {
            if (contract.EndDate <= contract.StartDate)
            {
                ModelState.AddModelError(nameof(contract.EndDate), "End date must be after the start date.");
            }

            if (!ModelState.IsValid)
            {
                LoadClients(contract.ClientId);
                return View(contract);
            }

            contract.Status = ContractStatus.Draft;
            _context.Add(contract);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Contract created as Draft. Activate it when it is ready.";
            return RedirectToAction(nameof(Details), new { id = contract.ContractId });
        }

        // GET: Contracts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound();

            LoadClients(contract.ClientId);
            return View(contract);
        }

        // POST: Contracts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ContractId,ClientId,StartDate,EndDate,ServiceLevel")] Contract input)
        {
            if (id != input.ContractId) return NotFound();

            if (input.EndDate <= input.StartDate)
            {
                ModelState.AddModelError(nameof(input.EndDate), "End date must be after the start date.");
            }

            if (!ModelState.IsValid)
            {
                LoadClients(input.ClientId);
                return View(input);
            }

            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound();

            // Only these fields can be edited; status changes go through the State pattern
            contract.ClientId = input.ClientId;
            contract.StartDate = input.StartDate;
            contract.EndDate = input.EndDate;
            contract.ServiceLevel = input.ServiceLevel;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Contract updated.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Contracts/ChangeStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int id, string command)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound();
            AttachObservers(contract);

            try
            {
                switch (command)
                {
                    case "activate":
                        contract.Activate();
                        break;
                    case "hold":
                        contract.PutOnHold();
                        break;
                    case "expire":
                        contract.Expire();
                        break;
                    default:
                        return BadRequest();
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = $"Contract status changed to {contract.Status}.";
            }
            catch (InvalidOperationException ex)
            {
                // The state class refused the change, so show its reason
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }
        // POST: Contracts/UploadAgreement/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadAgreement(int id, IFormFile? agreementFile)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound();

            if (agreementFile == null)
            {
                TempData["Error"] = "Please choose a PDF file to upload.";
                return RedirectToAction(nameof(Details), new { id });
            }

            try
            {
                var storedFileName = await _files.SaveAgreementAsync(agreementFile, id);

                // Remove the previous agreement only after the new one saved successfully
                _files.DeleteAgreement(contract.SignedAgreementPath);

                contract.SignedAgreementPath = storedFileName;
                await _context.SaveChangesAsync();

                TempData["Success"] = "Signed agreement uploaded.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: Contracts/DownloadAgreement/5
        public async Task<IActionResult> DownloadAgreement(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            var fullPath = _files.GetAgreementPath(contract?.SignedAgreementPath);

            if (contract == null || fullPath == null)
            {
                TempData["Error"] = "No signed agreement has been uploaded for this contract.";
                return RedirectToAction(nameof(Details), new { id });
            }

            return PhysicalFile(fullPath, "application/pdf", $"Contract-{id}-Signed-Agreement.pdf");
        }
        // GET: Contracts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var contract = await _context.Contracts
                .Include(c => c.Client)
                .FirstOrDefaultAsync(c => c.ContractId == id);

            if (contract == null) return NotFound();

            return View(contract);
        }

        // POST: Contracts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract != null)
            {
                _files.DeleteAgreement(contract.SignedAgreementPath);
                _context.Contracts.Remove(contract);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Contract deleted.";
            }

            return RedirectToAction(nameof(Index));
        }

        private void LoadClients(int? selectedId = null)
        {
            ViewData["ClientId"] = new SelectList(
                _context.Clients.OrderBy(c => c.Name), "ClientId", "Name", selectedId);
        }

        private void AttachObservers(Contract contract)
        {
            contract.Attach(new AuditLogObserver(_context));
            contract.Attach(new ManagerAlertObserver(_context));
        }
    }
}