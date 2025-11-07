using barbearia.api.Data;
using barbearia.api.Models;
using Microsoft.EntityFrameworkCore;

namespace barbearia.api.Services
{
    public class AvailabilityService : IAvailabilityService
    {
        private readonly AppDbContext _context;

        // O serviço precisa do DbContext para acessar o banco
        public AvailabilityService(AppDbContext context)
        {
            _context = context;
        }

        // O método que faz o trabalho pesado
        public async Task<List<TimeSpan>> GetAvailableSlotsAsync(int barberId, List<int> serviceIds, DateTime date)
        {
            // --- TODA A LÓGICA QUE ESTAVA NO CONTROLLER VEM PARA CÁ ---

            // 1. Calcular Duração Total
            int totalDuration = 0;
            var services = await _context.Services
                                   .Where(s => serviceIds.Contains(s.Id))
                                   .ToListAsync();
            if (services.Count != serviceIds.Count)
            {
                // Lançar uma exceção ou retornar lista vazia se algum serviço não for encontrado
                throw new ArgumentException("Um ou mais IDs de serviço são inválidos.");
            }
            totalDuration = services.Sum(s => s.DurationInMinutes);


            // 2. Encontrar Horário de Trabalho
            var dayOfWeek = date.DayOfWeek;
            var workSchedule = await _context.WorkSchedules
                .FirstOrDefaultAsync(s => s.BarberId == barberId && s.DayOfWeek == dayOfWeek);

            if (workSchedule == null)
            {
                return new List<TimeSpan>(); // Barbeiro não trabalha
            }

            // 3. Buscar Agendamentos Existentes (com a correção UTC)
            var startDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
            var endDate = startDate.AddDays(1);
            var existingAppointments = await _context.Appointments
                .Where(a => a.BarberId == barberId &&
                            a.StartDateTime >= startDate &&
                            a.StartDateTime < endDate &&
                            a.Status == AppointmentStatus.Scheduled)
                .ToListAsync();

            // 4. Gerar Slots Disponíveis (a mesma lógica de antes)
            var availableSlots = new List<TimeSpan>();
            var slotInterval = TimeSpan.FromMinutes(15);
            var currentSlot = workSchedule.StartTime;

            while (currentSlot < workSchedule.EndTime)
            {
                var slotEndTime = currentSlot.Add(TimeSpan.FromMinutes(totalDuration));

                if (slotEndTime > workSchedule.EndTime) break;

                bool inBreak = (currentSlot < workSchedule.BreakEndTime &&
                                slotEndTime > workSchedule.BreakStartTime);

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
