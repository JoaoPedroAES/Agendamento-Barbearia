using barbearia.api.Data;
using barbearia.api.Models;
using barbearia.api.Services;
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

        public ServicesController(IServicesService servicesService) // <-- Recebe o serviço
        {
            _servicesService = servicesService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Service>>> GetServices()
        {
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
        public async Task<ActionResult<Service>> CreateService(Service service)
        {
            // Validação básica do modelo (pode ser mais robusta no serviço se necessário)
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdService = await _servicesService.CreateServiceAsync(service);
            // Retorna 201 Created com a rota para o novo recurso
            return CreatedAtAction(nameof(GetService), new { id = createdService.Id }, createdService);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Barbeiro")]
        public async Task<IActionResult> UpdateService(int id, Service serviceInput)
        {
            // Validação básica
            if (id != serviceInput.Id) // Garante que o ID da rota e do corpo coincidem
            {
                return BadRequest("O ID na URL deve ser igual ao ID no corpo da requisição.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var success = await _servicesService.UpdateServiceAsync(id, serviceInput);
            if (!success)
            {
                return NotFound(); // 404 se o serviço não existir
            }
            return NoContent(); // 204 Sucesso (sem conteúdo no corpo da resposta)
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
