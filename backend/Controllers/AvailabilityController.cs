using barbearia.api.Data;
using barbearia.api.Models;
using barbearia.api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace barbearia.api.Controllers
{
    [Route("api/availability")]
    [ApiController]
    public class AvailabilityController : ControllerBase
    {
        private readonly IAvailabilityService _availabilityService;
        public AvailabilityController(IAvailabilityService availabilityService)
        {
            _availabilityService = availabilityService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailability(
            [FromQuery] int barberId,
            [FromQuery] List<int> serviceIds,
            [FromQuery] DateTime date)
        {
            try
            {
                var availableSlots = await _availabilityService.GetAvailableSlotsAsync(barberId, serviceIds, date);
                return Ok(availableSlots);
            }
            catch (ArgumentException ex) // Captura erros específicos do serviço
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex) // Captura erros genéricos
            {
                Console.WriteLine($"Erro inesperado em GetAvailability: {ex.Message}");
                return StatusCode(500, "Ocorreu um erro interno ao buscar a disponibilidade.");
            }
        }

    }
}
