// Controllers/WorkScheduleController.cs

using barbearia.api.Data;
using barbearia.api.Dtos;
using barbearia.api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace barbearia.api.Controllers
{
    [Route("api/work-schedule")]
    [ApiController]
    
    public class WorkScheduleController : ControllerBase
    {
        private readonly AppDbContext _context;
        public WorkScheduleController(AppDbContext context)
        {
            _context = context;
        }

        
        [HttpPost]
        [Authorize(Roles = "Admin,Barbeiro")]
        public async Task<IActionResult> SetWorkSchedule([FromBody] SetWorkScheduleDto dto)
        {
            
            var schedule = await _context.WorkSchedules
                .FirstOrDefaultAsync(s => s.BarberId == dto.BarberId && s.DayOfWeek == dto.DayOfWeek);

            if (schedule == null)
            {
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
                schedule.StartTime = dto.StartTime;
                schedule.EndTime = dto.EndTime;
                schedule.BreakStartTime = dto.BreakStartTime;
                schedule.BreakEndTime = dto.BreakEndTime;
            }

            await _context.SaveChangesAsync();

            var scheduleWithBarber = await _context.WorkSchedules
                .Include(s => s.Barber)
                    .ThenInclude(b => b.UserAccount)
                .FirstOrDefaultAsync(s => s.Id == schedule.Id);

            return Ok(scheduleWithBarber);
        }

        
        [HttpGet("{barberId:int}")]
        [Authorize]
        public async Task<IActionResult> GetSchedule(int barberId)
        {
            
            var schedules = await _context.WorkSchedules
                .Where(s => s.BarberId == barberId)
                .Include(s => s.Barber)
                    .ThenInclude(b => b.UserAccount)
                .ToListAsync();

            return Ok(schedules);
        }
    }
}