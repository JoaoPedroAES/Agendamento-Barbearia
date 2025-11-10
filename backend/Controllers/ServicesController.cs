using barbearia.api.Data;
using barbearia.api.Models;
using barbearia.api.Services;
using barbearia.api.Dtos; // <-- 1. IMPORTE OS DTOs
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
        private readonly IServicesService _servicesService;

        public ServicesController(IServicesService servicesService)
        {
            _servicesService = servicesService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Service>>> GetServices()
        {
            // Corrigido para o nome do método na interface
            var services = await _servicesService.GetAllServicesAsync();
            return Ok(services);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Service>> GetService(int id)
        {
            var service = await _servicesService.GetServiceByIdAsync(id);
            if (service == null)
            {
                return NotFound(); // 404
            }
            return Ok(service);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Barbeiro")]
        // 2. ALTERADO para CreateServiceDto
        public async Task<ActionResult<Service>> CreateService(CreateServiceDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 3. ALTERADO para passar o DTO
            var createdService = await _servicesService.CreateServiceAsync(dto);
            return CreatedAtAction(nameof(GetService), new { id = createdService.Id }, createdService);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Barbeiro")]
        // 4. ALTERADO para UpdateServiceDto
        public async Task<IActionResult> UpdateService(int id, UpdateServiceDto dto)
        {
            // 5. REMOVIDO o "if (id != serviceInput.Id)" (DTO não tem ID)

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 6. ALTERADO para passar o DTO
            var updatedService = await _servicesService.UpdateServiceAsync(id, dto);
            if (updatedService == null)
            {
                return NotFound(); // 404 se o serviço não existir
            }
            return NoContent(); // 204 Sucesso
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Barbeiro")]
        public async Task<IActionResult> DeleteService(int id)
        {
            var success = await _servicesService.DeleteServiceAsync(id);
            if (!success)
            {
                return NotFound(); // 404 se não encontrado
            }
            return NoContent(); // 204 Sucesso
        }

    }
}