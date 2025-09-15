using barbearia.api.Data;
using barbearia.api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

namespace barbearia.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicesController : ControllerBase
    {
        private readonly Data.AppDbContext _context;

        // Construtor para Injeção de Dependência
        public ServicesController(AppDbContext context) {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Service>>> GetServices() { 
        
            var services = await _context.Services.ToListAsync();
            return Ok(services);

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Service>> GetServiceId(int id) { 
        
            var service = await _context.Services.FindAsync(id);
            if (service == null) {
                return NotFound("Id do Serviço não encontrado");
            }
            return Ok(service);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Service>> CreateService(Service Service) {
            _context.Services.Add(Service);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetServiceId), new { id = Service.Id }, Service);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateService(int id, Service serviceUpdate)
        {
            if (id != serviceUpdate.Id)
            {
                return BadRequest();
            }

            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                return NotFound();
            }

            // Atualiza os campos
            service.Name = serviceUpdate.Name;
            service.Description = serviceUpdate.Description;
            service.Price = serviceUpdate.Price;
            service.DurationInMinutes = serviceUpdate.DurationInMinutes;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteService(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                return NotFound();
            }

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}
