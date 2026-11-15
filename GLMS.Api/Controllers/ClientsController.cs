using GLMS.Api.Data;
using GLMS.Api.Dtos;
using GLMS.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientsController : ControllerBase
    {
        private readonly GlmsDbContext _context;

        public ClientsController(GlmsDbContext context)
        {
            _context = context;
        }

        // GET: api/clients
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClientDto>>> GetClients()
        {
            var clients = await _context.Clients.OrderBy(c => c.Name).ToListAsync();
            return Ok(clients.Select(c => c.ToDto()));
        }

        // GET: api/clients/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ClientDto>> GetById(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null) return NotFound(new { message = "Client not found." });

            return Ok(client.ToDto());
        }

        // POST: api/clients
        [HttpPost]
        public async Task<ActionResult<ClientDto>> Create(ClientDto dto)
        {
            var client = new Client
            {
                Name = dto.Name,
                ContactEmail = dto.ContactEmail,
                ContactPhone = dto.ContactPhone,
                Region = dto.Region
            };

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = client.ClientId }, client.ToDto());
        }

        // PUT: api/clients/5
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ClientDto>> Update(int id, ClientDto dto)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null) return NotFound(new { message = "Client not found." });

            client.Name = dto.Name;
            client.ContactEmail = dto.ContactEmail;
            client.ContactPhone = dto.ContactPhone;
            client.Region = dto.Region;

            await _context.SaveChangesAsync();
            return Ok(client.ToDto());
        }

        // DELETE: api/clients/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null) return NotFound(new { message = "Client not found." });

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}