// Controllers/WorkScheduleController.cs

using barbearia.api.Data;
using barbearia.api.Dtos;
using barbearia.api.Models;
using barbearia.api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace barbearia.api.Controllers
{
    [Route("api/work-schedule")]
    [ApiController]
    
    public class WorkScheduleController : ControllerBase
    {
        private readonly IWorkScheduleService _workScheduleService;

        public WorkScheduleController(IWorkScheduleService workScheduleService) // <-- Recebe o serviço
        {
            _workScheduleService = workScheduleService;
        }


        [HttpPost("batch")]
        [Authorize(Roles = "Admin,Barbeiro")]
        public async Task<IActionResult> SetBatchWorkSchedule([FromBody] List<SetWorkScheduleDto> scheduleList)
        {
            // Validação básica
            if (scheduleList == null || !scheduleList.Any())
            {
                return BadRequest("A lista de horários não pode ser vazia.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // TODO: Adicionar validação de segurança:
            // Um Barbeiro só pode alterar o próprio horário (verificar user.BarberId vs dto.BarberId)
            // Um Admin pode alterar qualquer um.

            try
            {
                await _workScheduleService.SetBatchWorkScheduleAsync(scheduleList);
                return NoContent(); // 204 Sucesso
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message); // Erro nos horários enviados
            }
            catch (Exception ex)
            {
                // Logar erro ex
                return StatusCode(500, "Erro ao salvar a agenda.");
            }
        }

        [HttpGet("{barberId:int}")]
        [Authorize]
        public async Task<IActionResult> GetSchedule(int barberId)
        {
            try
            {
                var schedule = await _workScheduleService.GetScheduleByBarberIdAsync(barberId);
                return Ok(schedule);
            }
            catch (Exception ex)
            {
                // Logar erro ex
                return StatusCode(500, "Erro ao buscar horários.");
            }
        }
    }
}