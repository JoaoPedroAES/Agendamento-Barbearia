using barbearia.api.Data;
using barbearia.api.Dtos;
using barbearia.api.Models;
using Microsoft.EntityFrameworkCore;

namespace barbearia.api.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly AppDbContext _context;
        private readonly IAvailabilityService _availabilityService;
        private readonly IEmailService _emailService;

        public AppointmentService(AppDbContext context, IAvailabilityService availabilityService, IEmailService emailService)
        {
            _context = context;
            _availabilityService = availabilityService;
            _emailService = emailService;
        }

        public async Task<Appointment> CreateAppointmentAsync(CreateAppointmentDto dto, string customerId)
        {
            // Busca os serviços selecionados no banco de dados
            var services = await _context.Services
                                        .Where(s => dto.ServiceIds.Contains(s.Id))
                                        .ToListAsync();

            // Verifica se todos os IDs de serviço são válidos
            if (services.Count != dto.ServiceIds.Count)
            {
                throw new ArgumentException("Um ou mais IDs de serviço são inválidos.");
            }

            // Calcula o preço total e a duração total dos serviços
            decimal totalPrice = services.Sum(s => s.Price);
            int totalDuration = services.Sum(s => s.DurationInMinutes);

            // Define o horário de início como UTC
            var startDateTimeUtc = DateTime.SpecifyKind(dto.StartDateTime, DateTimeKind.Utc);

            // Obtém os horários disponíveis para o barbeiro e os serviços
            var availableSlots = await _availabilityService.GetAvailableSlotsAsync(dto.BarberId, dto.ServiceIds, startDateTimeUtc.Date);

            // Verifica se o horário selecionado ainda está disponível
            if (!availableSlots.Contains(startDateTimeUtc.TimeOfDay))
            {
                throw new InvalidOperationException("O horário selecionado não está mais disponível.");
            }

            // Cria o objeto de agendamento
            var appointment = new Appointment
            {
                CustomerId = customerId,
                BarberId = dto.BarberId,
                StartDateTime = startDateTimeUtc,
                EndDateTime = startDateTimeUtc.AddMinutes(totalDuration),
                Status = AppointmentStatus.Scheduled,
                TotalPrice = totalPrice,
                Services = services // Serviços associados ao agendamento
            };

            // Salva o agendamento no banco de dados
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            // Tenta enviar o e-mail de confirmação
            try
            {
                var cliente = await _context.Users.FindAsync(customerId);
                var barbeiro = await _context.Barbers
                    .Include(b => b.UserAccount) // Inclui os dados do barbeiro
                    .FirstOrDefaultAsync(b => b.Id == dto.BarberId);

                // Verifica se os dados necessários para o e-mail estão completos
                if (cliente != null && barbeiro != null && appointment.Services.Any())
                {
                    await _emailService.EnviarEmailConfirmacaoAgendamento(appointment, cliente, barbeiro);
                }
                else
                {
                    Console.WriteLine("Erro: Dados incompletos para enviar e-mail de confirmação.");
                }
            }
            catch (Exception ex)
            {
                // Loga o erro, mas não cancela o agendamento
                Console.WriteLine($"Erro ao disparar e-mail de agendamento: {ex.Message}");
            }

            return appointment;
        }

        public async Task<IEnumerable<Appointment>> GetMyAppointmentsAsync(string customerId)
        {
            // Retorna os agendamentos do cliente, incluindo barbeiro e serviços
            return await _context.Appointments
                .Where(a => a.CustomerId == customerId)
                .Include(a => a.Barber)
                    .ThenInclude(b => b.UserAccount) // Inclui os dados do barbeiro
                .Include(a => a.Services)
                .OrderByDescending(a => a.StartDateTime)
                .ToListAsync();
        }

        public async Task<bool> CancelAppointmentAsync(int appointmentId, string customerId)
        {
            // Busca o agendamento e os dados necessários para o e-mail
            var appointment = await _context.Appointments
                .Include(a => a.Customer)      // Inclui os dados do cliente
                .Include(a => a.Barber)        // Inclui os dados do barbeiro
                    .ThenInclude(b => b.UserAccount) // Inclui a conta do barbeiro
                .Include(a => a.Services)      // Inclui os serviços do agendamento
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            // Verifica se o agendamento existe e pertence ao cliente
            if (appointment == null || appointment.CustomerId != customerId)
            {
                return false; // Não encontrado ou não pertence ao cliente
            }

            // Verifica se o agendamento já foi cancelado
            if (appointment.Status == AppointmentStatus.CancelledByCustomer)
            {
                return true; // Já estava cancelado
            }

            // Altera o status para cancelado
            appointment.Status = AppointmentStatus.CancelledByCustomer;
            await _context.SaveChangesAsync(); // Salva a alteração no banco

            // Tenta enviar o e-mail de cancelamento
            try
            {
                await _emailService.EnviarEmailCancelamento(appointment, appointment.Customer, appointment.Barber);
            }
            catch (Exception ex)
            {
                // Loga o erro, mas não falha a operação
                Console.WriteLine($"Erro ao disparar e-mail de cancelamento: {ex.Message}");
            }

            return true;
        }

        public async Task<IEnumerable<Appointment>> GetAgendaAsync(DateTime date, string userId, bool isBarber)
        {
            // Define o intervalo de data para a agenda
            var startDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
            var endDate = startDate.AddDays(1);
            var query = _context.Appointments.AsQueryable();

            // Filtra os agendamentos se o usuário for barbeiro
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

            // Retorna os agendamentos no intervalo de data, incluindo cliente e serviços
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