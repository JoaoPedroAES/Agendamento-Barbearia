using barbearia.api.Data;
using barbearia.api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace barbearia.api.Controllers
{
    [Route("api/availability")]
    [ApiController]
    public class AvailabilityController : ControllerBase
    {
        private readonly AppDbContext _context;
        public AvailabilityController(AppDbContext context) { _context = context; }

        [HttpGet]
        public async Task<IActionResult> GetAvailability(
            [FromQuery] int barberId,
            [FromQuery] List<int> serviceIds,
            [FromQuery] DateTime date)
        {
            // 1. Calcular Duração Total dos Serviços
            int totalDuration = 0;
            foreach (var serviceId in serviceIds)
            {
                var service = await _context.Services.FindAsync(serviceId);
                if (service == null) return NotFound($"Serviço {serviceId} não encontrado.");
                totalDuration += service.DurationInMinutes;
            }


            var dayOfWeek = date.DayOfWeek;
            var workSchedule = await _context.WorkSchedules
                .FirstOrDefaultAsync(s => s.BarberId == barberId && s.DayOfWeek == dayOfWeek);

            if (workSchedule == null)
            {
                return Ok(new List<string>()); // Barbeiro não trabalha neste dia
            }

            var startDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
            var endDate = startDate.AddDays(1);

            var existingAppointments = await _context.Appointments
                .Where(a => a.BarberId == barberId &&
                a.StartDateTime >= startDate &&
                a.StartDateTime < endDate &&
                a.Status == AppointmentStatus.Scheduled).ToListAsync();

            // 4. Gerar Slots Disponíveis (Lógica Principal)
            var availableSlots = new List<TimeSpan>();
            var slotInterval = TimeSpan.FromMinutes(15); // Define a granularidade (ex: 9:00, 9:15, 9:30)
            var currentSlot = workSchedule.StartTime;

            while (currentSlot < workSchedule.EndTime)
            {
                var slotEndTime = currentSlot.Add(TimeSpan.FromMinutes(totalDuration));

                // 4a. Verifica se o slot termina antes do fim do dia
                if (slotEndTime > workSchedule.EndTime)
                {
                    break; // Não cabe mais neste dia
                }

                // 4b. Verifica se o slot cai DENTRO do horário de almoço
                bool inBreak = (currentSlot < workSchedule.BreakEndTime &&
                                slotEndTime > workSchedule.BreakStartTime);

                // 4c. Verifica se o slot colide com agendamentos existentes
                bool overlapsExisting = existingAppointments.Any(a =>
                    currentSlot < a.EndDateTime.TimeOfDay &&
                    slotEndTime > a.StartDateTime.TimeOfDay
                );

                if (!inBreak && !overlapsExisting)
                {
                    availableSlots.Add(currentSlot);
                }

                currentSlot = currentSlot.Add(slotInterval);
            }

            return Ok(availableSlots); // Retorna lista de horários (ex: ["09:00:00", "09:45:00"])
        }
    }
}
