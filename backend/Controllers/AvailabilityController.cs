using barbearia.api.Services;
using Microsoft.AspNetCore.Mvc;

namespace barbearia.api.Controllers
{
    [Route("api/availability")]
    [ApiController]
    public class AvailabilityController : ControllerBase
    {
        private readonly IAvailabilityService _availabilityService;

        // Construtor que injeta o serviço de disponibilidade
        public AvailabilityController(IAvailabilityService availabilityService)
        {
            _availabilityService = availabilityService;
        }

        // Endpoint para obter os horários disponíveis de um barbeiro
        [HttpGet]
        public async Task<IActionResult> GetAvailability(
            [FromQuery] int barberId, // ID do barbeiro
            [FromQuery] List<int> serviceIds, // IDs dos serviços selecionados
            [FromQuery] DateTime date) // Data para verificar a disponibilidade
        {
            try
            {
                // Chama o serviço para calcular os horários disponíveis
                var availableSlots = await _availabilityService.GetAvailableSlotsAsync(barberId, serviceIds, date);
                return Ok(availableSlots); // Retorna 200 com os horários disponíveis
            }
            catch (ArgumentException ex) // Captura erros específicos do serviço
            {
                return BadRequest(ex.Message); // Retorna 400 com a mensagem de erro
            }
            catch (Exception ex) // Captura erros genéricos
            {
                // Loga o erro para depuração
                Console.WriteLine($"Erro inesperado em GetAvailability: {ex.Message}");
                // Retorna 500 para erros internos
                return StatusCode(500, "Ocorreu um erro interno ao buscar a disponibilidade.");
            }
        }
    }
}
