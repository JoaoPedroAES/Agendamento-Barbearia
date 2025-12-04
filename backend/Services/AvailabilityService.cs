using barbearia.api.Data;
using barbearia.api.Models;
using Microsoft.EntityFrameworkCore;

namespace barbearia.api.Services
{
    public class AvailabilityService : IAvailabilityService
    {
        private readonly AppDbContext _context;

        public AvailabilityService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<TimeSpan>> GetAvailableSlotsAsync(int barberId, List<int> serviceIds, DateTime date)
        {
            // 1. Calcula a duração total
            int totalDuration = 0;
            var services = await _context.Services
                                       .Where(s => serviceIds.Contains(s.Id))
                                       .ToListAsync();

            if (services.Count != serviceIds.Count)
            {
                throw new ArgumentException("Um ou mais IDs de serviço são inválidos.");
            }
            totalDuration = services.Sum(s => s.DurationInMinutes);

            // 2. Obtém o horário de trabalho
            var dayOfWeek = date.DayOfWeek;
            var workSchedule = await _context.WorkSchedules
                .FirstOrDefaultAsync(s => s.BarberId == barberId && s.DayOfWeek == dayOfWeek);

            if (workSchedule == null)
            {
                return new List<TimeSpan>();
            }

            // 3. Busca agendamentos (CORREÇÃO DE SEGURANÇA AQUI)
            var startDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
            var endDate = startDate.AddDays(1);

            var existingAppointments = await _context.Appointments
                .Where(a => a.BarberId == barberId &&
                            a.StartDateTime >= startDate &&
                            a.StartDateTime < endDate &&
                            // CORREÇÃO: Ignoramos os dois tipos de cancelamento que você tem
                            a.Status != AppointmentStatus.CancelledByCustomer &&
                            a.Status != AppointmentStatus.CancelledByAdmin)
                .ToListAsync();

            // 4. Gera os horários (Slots)
            var availableSlots = new List<TimeSpan>();
            var slotInterval = TimeSpan.FromMinutes(15);
            var currentSlot = workSchedule.StartTime;

            while (currentSlot < workSchedule.EndTime)
            {
                var slotEndTime = currentSlot.Add(TimeSpan.FromMinutes(totalDuration));

                // Verifica limites do expediente
                if (slotEndTime > workSchedule.EndTime) break;

                // Verifica Pausa (Almoço)
                bool inBreak = (currentSlot < workSchedule.BreakEndTime &&
                                slotEndTime > workSchedule.BreakStartTime);

                // Verifica Conflitos com outros agendamentos
                // (Nota: Como filtramos os Cancelados na busca, eles não entram aqui, liberando o horário!)
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

            return availableSlots;
        }
    }
}