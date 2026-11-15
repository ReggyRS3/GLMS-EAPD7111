using GLMS.Api.Data;
using GLMS.Api.Dtos;
using GLMS.Api.Models;
using GLMS.Api.Patterns.Observer;
using GLMS.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContractsController : ControllerBase
    {
        private readonly GlmsDbContext _context;
        private readonly IFileStorageService _files;

        public ContractsController(GlmsDbContext context, IFileStorageService files)
        {
            _context = context;
            _files = files;
        }

        // GET: api/contracts?fromDate=2026-01-01&toDate=2026-12-31&status=Active
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ContractDto>>> GetContracts(
            [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, [FromQuery] ContractStatus? status)
        {
            await ExpireOverdueContractsAsync();

            if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
            {
                return BadRequest(new { message = "The 'From' date must be before the 'To' date." });
            }

            IQueryable<Contract> query = _context.Contracts.Include(c => c.Client);
            query = ContractFilter.Apply(query, fromDate, toDate, status);

            var contracts = await query.OrderBy(c => c.EndDate).ToListAsync();
            return Ok(contracts.Select(c => c.ToDto()));
        }

        // GET: api/contracts/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ContractDto>> GetById(int id)
        {
            var contract = await _context.Contracts
                .Include(c => c.Client)
                .FirstOrDefaultAsync(c => c.ContractId == id);

            if (contract == null) return NotFound(new { message = "Contract not found." });

            return Ok(contract.ToDto());
        }

        // GET: api/contracts/5/history
        [HttpGet("{id:int}/history")]
        public async Task<ActionResult<IEnumerable<ContractHistoryDto>>> GetHistory(int id)
        {
            var history = await _context.ContractAuditLogs
                .Where(l => l.ContractId == id)
                .OrderByDescending(l => l.ChangedAt)
                .Select(l => new ContractHistoryDto
                {
                    ChangedAt = l.ChangedAt,
                    OldStatus = l.OldStatus,
                    NewStatus = l.NewStatus
                })
                .ToListAsync();

            return Ok(history);
        }

        // POST: api/contracts
        [HttpPost]
        public async Task<ActionResult<ContractDto>> Create(ContractSaveDto dto)
        {
            if (dto.EndDate <= dto.StartDate)
            {
                return BadRequest(new { message = "End date must be after the start date." });
            }

            if (!await _context.Clients.AnyAsync(c => c.ClientId == dto.ClientId))
            {
                return BadRequest(new { message = "The selected client does not exist." });
            }

            var contract = new Contract
            {
                ClientId = dto.ClientId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                ServiceLevel = dto.ServiceLevel,
                Status = ContractStatus.Draft // new contracts always start as Draft
            };

            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();
            await _context.Entry(contract).Reference(c => c.Client).LoadAsync();

            return CreatedAtAction(nameof(GetById), new { id = contract.ContractId }, contract.ToDto());
        }

        // PUT: api/contracts/5  (status changes use PATCH instead)
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ContractDto>> Update(int id, ContractSaveDto dto)
        {
            if (dto.EndDate <= dto.StartDate)
            {
                return BadRequest(new { message = "End date must be after the start date." });
            }

            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound(new { message = "Contract not found." });

            contract.ClientId = dto.ClientId;
            contract.StartDate = dto.StartDate;
            contract.EndDate = dto.EndDate;
            contract.ServiceLevel = dto.ServiceLevel;

            await _context.SaveChangesAsync();
            await _context.Entry(contract).Reference(c => c.Client).LoadAsync();

            return Ok(contract.ToDto());
        }

        // PATCH: api/contracts/5/status   body: { "action": "Activate" }
        [HttpPatch("{id:int}/status")]
        public async Task<ActionResult<ContractDto>> ChangeStatus(int id, ContractStatusUpdateDto dto)
        {
            var contract = await _context.Contracts
                .Include(c => c.Client)
                .FirstOrDefaultAsync(c => c.ContractId == id);

            if (contract == null) return NotFound(new { message = "Contract not found." });

            AttachObservers(contract);

            try
            {
                switch (dto.Action!.Value)
                {
                    case ContractAction.Activate: contract.Activate(); break;
                    case ContractAction.PutOnHold: contract.PutOnHold(); break;
                    case ContractAction.Expire: contract.Expire(); break;
                }
            }
            catch (InvalidOperationException ex)
            {
                // The State pattern refused the change
                return Conflict(new { message = ex.Message });
            }

            await _context.SaveChangesAsync();
            return Ok(contract.ToDto());
        }

        // POST: api/contracts/5/agreement  (multipart/form-data, field "file")
        [HttpPost("{id:int}/agreement")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ContractDto>> UploadAgreement(int id, IFormFile? file)
        {
            var contract = await _context.Contracts
                .Include(c => c.Client)
                .FirstOrDefaultAsync(c => c.ContractId == id);

            if (contract == null) return NotFound(new { message = "Contract not found." });
            if (file == null) return BadRequest(new { message = "Please choose a PDF file to upload." });

            try
            {
                var storedFileName = await _files.SaveAgreementAsync(file, id);
                _files.DeleteAgreement(contract.SignedAgreementPath);
                contract.SignedAgreementPath = storedFileName;
                await _context.SaveChangesAsync();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }

            return Ok(contract.ToDto());
        }

        // GET: api/contracts/5/agreement
        [HttpGet("{id:int}/agreement")]
        public async Task<IActionResult> DownloadAgreement(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            var fullPath = _files.GetAgreementPath(contract?.SignedAgreementPath);

            if (contract == null || fullPath == null)
            {
                return NotFound(new { message = "No signed agreement has been uploaded for this contract." });
            }

            return PhysicalFile(fullPath, "application/pdf", $"Contract-{id}-Signed-Agreement.pdf");
        }

        // DELETE: api/contracts/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound(new { message = "Contract not found." });

            _files.DeleteAgreement(contract.SignedAgreementPath);
            _context.Contracts.Remove(contract);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Automated status tracking: contracts past their end date are expired
        private async Task ExpireOverdueContractsAsync()
        {
            var overdue = await _context.Contracts
                .Where(c => c.EndDate < DateTime.Today &&
                           (c.Status == ContractStatus.Active || c.Status == ContractStatus.OnHold))
                .ToListAsync();

            foreach (var contract in overdue)
            {
                AttachObservers(contract);
                contract.Expire();
            }

            if (overdue.Count > 0)
            {
                await _context.SaveChangesAsync();
            }
        }

        private void AttachObservers(Contract contract)
        {
            contract.Attach(new AuditLogObserver(_context));
            contract.Attach(new ManagerAlertObserver(_context));
        }
    }
}