using barbearia.api.Data;
using barbearia.api.Models;
using Microsoft.EntityFrameworkCore;

namespace barbearia.api.Services
{
    public class AvailabilityService : IAvailabilityService
    {
        private readonly AppDbContext _context;

        // Construtor que recebe o DbContext para acessar o banco de dados
        public AvailabilityService(AppDbContext context)
        {
            _context = context;
        }

        // Método para obter os horários disponíveis para um barbeiro em uma data específica
        public async Task<List<TimeSpan>> GetAvailableSlotsAsync(int barberId, List<int> serviceIds, DateTime date)
        {
            // Calcula a duração total dos serviços selecionados
            int totalDuration = 0;
            var services = await _context.Services
                                   .Where(s => serviceIds.Contains(s.Id))
                                   .ToListAsync();

            // Verifica se todos os IDs de serviço são válidos
            if (services.Count != serviceIds.Count)
            {
                throw new ArgumentException("Um ou mais IDs de serviço são inválidos.");
            }
            totalDuration = services.Sum(s => s.DurationInMinutes);

            // Obtém o horário de trabalho do barbeiro para o dia da semana
            var dayOfWeek = date.DayOfWeek;
            var workSchedule = await _context.WorkSchedules
                .FirstOrDefaultAsync(s => s.BarberId == barberId && s.DayOfWeek == dayOfWeek);

            // Retorna uma lista vazia se o barbeiro não trabalha nesse dia
            if (workSchedule == null)
            {
                return new List<TimeSpan>();
            }

            // Busca os agendamentos existentes do barbeiro para a data especificada
            var startDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
            var endDate = startDate.AddDays(1);
            var existingAppointments = await _context.Appointments
                .Where(a => a.BarberId == barberId &&
                            a.StartDateTime >= startDate &&
                            a.StartDateTime < endDate &&
                            a.Status == AppointmentStatus.Scheduled)
                .ToListAsync();

            // Gera os horários disponíveis com base no horário de trabalho e nos agendamentos existentes
            var availableSlots = new List<TimeSpan>();
            var slotInterval = TimeSpan.FromMinutes(15); // Intervalo entre os horários disponíveis
            var currentSlot = workSchedule.StartTime;

            while (currentSlot < workSchedule.EndTime)
            {
                var slotEndTime = currentSlot.Add(TimeSpan.FromMinutes(totalDuration));

                // Verifica se o horário extrapola o horário de trabalho
                if (slotEndTime > workSchedule.EndTime) break;

                // Verifica se o horário está dentro do intervalo de pausa
                bool inBreak = (currentSlot < workSchedule.BreakEndTime &&
                                slotEndTime > workSchedule.BreakStartTime);

                // Verifica se o horário conflita com algum agendamento existente
                bool overlapsExisting = existingAppointments.Any(a =>
                    currentSlot < a.EndDateTime.TimeOfDay &&
                    slotEndTime > a.StartDateTime.TimeOfDay
                );

                // Adiciona o horário à lista se não estiver em pausa e não houver conflito
                if (!inBreak && !overlapsExisting)
                {
                    availableSlots.Add(currentSlot);
                }

                // Avança para o próximo horário
                currentSlot = currentSlot.Add(slotInterval);
            }

            return availableSlots; // Retorna a lista de horários disponíveis
        }
    }
}
