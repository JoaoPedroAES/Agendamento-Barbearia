using barbearia.api.Data;
using barbearia.api.Models;
using barbearia.api.Services;
using barbearia.api.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace barbearia.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicesController : ControllerBase
    {
        private readonly IServicesService _servicesService;

        // Construtor que injeta o serviço de gerenciamento de serviços
        public ServicesController(IServicesService servicesService)
        {
            _servicesService = servicesService;
        }

        // Endpoint para listar todos os serviços cadastrados
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Service>>> GetServices()
        {
            var services = await _servicesService.GetAllServicesAsync();
            return Ok(services); // Retorna 200 com a lista de serviços
        }

        // Endpoint para obter os dados de um serviço específico pelo ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Service>> GetService(int id)
        {
            var service = await _servicesService.GetServiceByIdAsync(id);
            if (service == null)
            {
                return NotFound(); // Retorna 404 se o serviço não for encontrado
            }
            return Ok(service); // Retorna 200 com os dados do serviço
        }

        // Endpoint para criar um novo serviço (somente Admin ou Barbeiro)
        [HttpPost]
        [Authorize(Roles = "Admin,Barbeiro")]
        public async Task<ActionResult<Service>> CreateService(CreateServiceDto dto)
        {
            // Valida o modelo recebido
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Retorna 400 se o modelo for inválido
            }

            // Chama o serviço para criar o novo serviço
            var createdService = await _servicesService.CreateServiceAsync(dto);
            // Retorna 201 Created com o serviço criado
            return CreatedAtAction(nameof(GetService), new { id = createdService.Id }, createdService);
        }

        // Endpoint para atualizar os dados de um serviço existente (somente Admin ou Barbeiro)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Barbeiro")]
        public async Task<IActionResult> UpdateService(int id, UpdateServiceDto dto)
        {
            // Valida o modelo recebido
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Retorna 400 se o modelo for inválido
            }

            // Chama o serviço para atualizar o serviço
            var updatedService = await _servicesService.UpdateServiceAsync(id, dto);
            if (updatedService == null)
            {
                return NotFound(); // Retorna 404 se o serviço não for encontrado
            }
            return NoContent(); // Retorna 204 para sucesso
        }

        // Endpoint para excluir um serviço pelo ID (somente Admin ou Barbeiro)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Barbeiro")]
        public async Task<IActionResult> DeleteService(int id)
        {
            // Chama o serviço para excluir o serviço
            var success = await _servicesService.DeleteServiceAsync(id);
            if (!success)
            {
                return NotFound(); // Retorna 404 se o serviço não for encontrado
            }
            return NoContent(); // Retorna 204 para sucesso
        }
    }
}