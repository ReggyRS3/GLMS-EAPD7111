using GLMS.Api.Data;
using GLMS.Api.Dtos;
using GLMS.Api.Models;
using GLMS.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceRequestsController : ControllerBase
    {
        private readonly GlmsDbContext _context;
        private readonly ServiceRequestService _requestService;

        public ServiceRequestsController(GlmsDbContext context, ServiceRequestService requestService)
        {
            _context = context;
            _requestService = requestService;
        }

        // GET: api/servicerequests
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceRequestDto>>> GetAll()
        {
            var requests = await _context.ServiceRequests
                .Include(s => s.Contract)
                    .ThenInclude(c => c!.Client)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return Ok(requests.Select(s => s.ToDto()));
        }

        // GET: api/servicerequests/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ServiceRequestDto>> GetById(int id)
        {
            var request = await LoadAsync(id);
            if (request == null) return NotFound(new { message = "Service request not found." });

            return Ok(request.ToDto());
        }

        // POST: api/servicerequests
        [HttpPost]
        public async Task<ActionResult<ServiceRequestDto>> Create(ServiceRequestCreateDto dto)
        {
            try
            {
                // Checks the contract is Active (State) and converts USD to ZAR (Adapter)
                var created = await _requestService.CreateRequestAsync(dto.ContractId, dto.Description, dto.CostUsd);
                var saved = await LoadAsync(created.ServiceRequestId);

                return CreatedAtAction(nameof(GetById), new { id = created.ServiceRequestId }, saved!.ToDto());
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/servicerequests/5  (description and status only)
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ServiceRequestDto>> Update(int id, ServiceRequestUpdateDto dto)
        {
            var request = await LoadAsync(id);
            if (request == null) return NotFound(new { message = "Service request not found." });

            request.Description = dto.Description;
            request.Status = dto.Status!.Value;
            await _context.SaveChangesAsync();

            return Ok(request.ToDto());
        }

        // DELETE: api/servicerequests/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var request = await _context.ServiceRequests.FindAsync(id);
            if (request == null) return NotFound(new { message = "Service request not found." });

            _context.ServiceRequests.Remove(request);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private Task<ServiceRequest?> LoadAsync(int id) =>
            _context.ServiceRequests
                .Include(s => s.Contract)
                    .ThenInclude(c => c!.Client)
                .FirstOrDefaultAsync(s => s.ServiceRequestId == id);
    }
}