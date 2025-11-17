using barbearia.api.Dtos;
using barbearia.api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace barbearia.api.Controllers
{
    [Route("api/work-schedule")]
    [ApiController]
    public class WorkScheduleController : ControllerBase
    {
        private readonly IWorkScheduleService _workScheduleService;

        // Construtor que injeta o serviço de horários de trabalho
        public WorkScheduleController(IWorkScheduleService workScheduleService)
        {
            _workScheduleService = workScheduleService;
        }

        // Endpoint para definir ou atualizar em lote os horários de trabalho de um barbeiro
        [HttpPost("batch")]
        [Authorize(Roles = "Admin,Barbeiro")]
        public async Task<IActionResult> SetBatchWorkSchedule([FromBody] List<SetWorkScheduleDto> scheduleList)
        {
            // Valida se a lista de horários foi enviada e não está vazia
            if (scheduleList == null || !scheduleList.Any())
            {
                return BadRequest("A lista de horários não pode ser vazia."); // Retorna 400 se a lista for inválida
            }

            // Valida o modelo recebido
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Retorna 400 se o modelo for inválido
            }

            // TODO: Adicionar validação de segurança:
            // Um barbeiro só pode alterar o próprio horário (verificar user.BarberId vs dto.BarberId)
            // Um administrador pode alterar qualquer horário.

            try
            {
                // Chama o serviço para salvar ou atualizar os horários
                await _workScheduleService.SetBatchWorkScheduleAsync(scheduleList);
                return NoContent(); // Retorna 204 para sucesso
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message); // Retorna 400 se houver erro nos horários enviados
            }
            catch (Exception ex)
            {
                // Loga o erro e retorna 500 para erros internos
                return StatusCode(500, "Erro ao salvar a agenda.");
            }
        }

        // Endpoint para obter os horários de trabalho de um barbeiro específico pelo ID
        [HttpGet("{barberId:int}")]
        [Authorize]
        public async Task<IActionResult> GetSchedule(int barberId)
        {
            try
            {
                // Chama o serviço para buscar os horários do barbeiro
                var schedule = await _workScheduleService.GetScheduleByBarberIdAsync(barberId);
                return Ok(schedule); // Retorna 200 com os horários encontrados
            }
            catch (Exception ex)
            {
                // Loga o erro e retorna 500 para erros internos
                return StatusCode(500, "Erro ao buscar horários.");
            }
        }
    }
}