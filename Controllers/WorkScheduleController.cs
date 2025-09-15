using barbearia.api.Data;
using barbearia.api.Dtos;
using barbearia.api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace barbearia.api.Controllers
{
    [Route("api/work-schedule")]
    [ApiController]
    [Authorize(Roles = "Admin,Barbeiro")]
    public class WorkScheduleController : ControllerBase
    {
        private readonly AppDbContext _context;
        public WorkScheduleController(AppDbContext context)
        {
            _context = context;
        }

        // Define ou atualiza o horário de um dia para um barbeiro
        [HttpPost]
        public async Task<IActionResult> SetWorkSchedule([FromBody] SetWorkScheduleDto dto)
        {
            // (Verificação de segurança: um barbeiro só pode mudar o próprio horário)

            var schedule = await _context.WorkSchedules.FirstOrDefaultAsync(s => s.BarberId == dto.BarberId && s.DayOfWeek == dto.DayOfWeek);

            if (schedule == null)
            {
                // Cria novo
                schedule = new WorkSchedule
                {
                    BarberId = dto.BarberId,
                    DayOfWeek = dto.DayOfWeek,
                    StartTime = dto.StartTime,
                    EndTime = dto.EndTime,
                    BreakStartTime = dto.BreakStartTime,
                    BreakEndTime = dto.BreakEndTime
                };
                _context.WorkSchedules.Add(schedule);
            }
            else
            {
                // Atualiza existente
                schedule.StartTime = dto.StartTime;
                schedule.EndTime = dto.EndTime;
                schedule.BreakStartTime = dto.BreakStartTime;
                schedule.BreakEndTime = dto.BreakEndTime;
            }

            await _context.SaveChangesAsync();
            return Ok(schedule);
        }

        // Pega o horário de um barbeiro específico
        [HttpGet("{barberId:int}")]
        public async Task<IActionResult> GetSchedule(int barberId)
        {
            var schedule = await _context.WorkSchedules
                .Where(s => s.BarberId == barberId)
                .ToListAsync();
            return Ok(schedule);
        }
    }
}
