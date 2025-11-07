using barbearia.api.Data;
using barbearia.api.Dtos;
using barbearia.api.Models;
using Microsoft.EntityFrameworkCore;

namespace barbearia.api.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly AppDbContext _context;
        // Injetamos o IAvailabilityService para revalidar o slot
        private readonly IAvailabilityService _availabilityService;

        public AppointmentService(AppDbContext context, IAvailabilityService availabilityService)
        {
            _context = context;
            _availabilityService = availabilityService;
        }

        public async Task<Appointment> CreateAppointmentAsync(CreateAppointmentDto dto, string customerId)
        {
            // 1. Buscar os serviços e calcular total/duração
            var services = await _context.Services
                                   .Where(s => dto.ServiceIds.Contains(s.Id))
                                   .ToListAsync();
            if (services.Count != dto.ServiceIds.Count)
            {
                throw new ArgumentException("Um ou mais IDs de serviço são inválidos.");
            }
            decimal totalPrice = services.Sum(s => s.Price);
            int totalDuration = services.Sum(s => s.DurationInMinutes);

            // --- REVALIDAÇÃO DO SLOT ---
            // 2. Chama o AvailabilityService para verificar se o slot AINDA está livre
            var startDateTimeUtc = DateTime.SpecifyKind(dto.StartDateTime, DateTimeKind.Utc);
            var availableSlots = await _availabilityService.GetAvailableSlotsAsync(dto.BarberId, dto.ServiceIds, startDateTimeUtc.Date);

            // Verifica se o horário exato ainda está na lista de disponíveis
            if (!availableSlots.Contains(startDateTimeUtc.TimeOfDay))
            {
                throw new InvalidOperationException("O horário selecionado não está mais disponível.");
            }
            // --- FIM DA REVALIDAÇÃO ---

            // 3. Criar e Salvar o Agendamento
            var appointment = new Appointment
            {
                CustomerId = customerId,
                BarberId = dto.BarberId,
                StartDateTime = startDateTimeUtc,
                EndDateTime = startDateTimeUtc.AddMinutes(totalDuration),
                Status = AppointmentStatus.Scheduled,
                TotalPrice = totalPrice,
                Services = services
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            // (TODO: Disparar e-mail de notificação aqui - poderia ser outro serviço)

            return appointment;
        }

        public async Task<IEnumerable<Appointment>> GetMyAppointmentsAsync(string customerId)
        {
            return await _context.Appointments
                .Where(a => a.CustomerId == customerId)
                .Include(a => a.Barber)
                    .ThenInclude(b => b.UserAccount) // Inclui dados do barbeiro
                .Include(a => a.Services)
                .OrderByDescending(a => a.StartDateTime)
                .ToListAsync();
        }

        public async Task<bool> CancelAppointmentAsync(int appointmentId, string customerId)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);

            if (appointment == null || appointment.CustomerId != customerId)
            {
                return false; // Não encontrado ou não pertence ao cliente
            }

            // Poderia adicionar regra de negócio (ex: só cancelar com X horas de antecedência)

            appointment.Status = AppointmentStatus.CancelledByCustomer;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Appointment>> GetAgendaAsync(DateTime date, string userId, bool isBarber)
        {
            var startDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
            var endDate = startDate.AddDays(1);
            var query = _context.Appointments.AsQueryable();

            if (isBarber)
            {
                var barberProfile = await _context.Barbers.FirstOrDefaultAsync(b => b.ApplicationUserId == userId);
                if (barberProfile != null)
                {
                    query = query.Where(a => a.BarberId == barberProfile.Id);
                }
                else
                {
                    return new List<Appointment>(); // Barbeiro sem perfil não tem agenda
                }
            }

            return await query
                .Where(a => a.StartDateTime >= startDate && a.StartDateTime < endDate)
                .Include(a => a.Customer)
                .Include(a => a.Services)
                .Include(a => a.Barber)
                    .ThenInclude(b => b.UserAccount)
                .OrderBy(a => a.StartDateTime)
                .ToListAsync();
        }
    }
}
