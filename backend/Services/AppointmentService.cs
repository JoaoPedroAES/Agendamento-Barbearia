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
        private readonly IEmailService _emailService; // <-- OK!

        // --- 1ª MUDANÇA: Adicionar IEmailService ao construtor ---
        public AppointmentService(AppDbContext context, IAvailabilityService availabilityService, IEmailService emailService)
        {
            _context = context;
            _availabilityService = availabilityService;
            _emailService = emailService; // <-- 2ª MUDANÇA: Atribuir o serviço
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
                Services = services // <-- Importante: os serviços já estão aqui
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync(); // <-- Agendamento salvo no banco!

            // --- 3ª MUDANÇA: Disparar o e-mail de notificação ---
            // Colocamos em um try/catch para que, se o e-mail falhar,
            // o agendamento não seja desfeito (o cliente conseguiu agendar).
            try
            {
                // Buscar os dados necessários para o e-mail
                var cliente = await _context.Users.FindAsync(customerId);

                var barbeiro = await _context.Barbers
                    .Include(b => b.UserAccount) // Inclui o usuário para pegar o e-mail
                    .FirstOrDefaultAsync(b => b.Id == dto.BarberId);

                // Verificamos se temos todos os dados para o e-mail
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
                // Logar o erro (ex: "Falha ao enviar e-mail de confirmação"), 
                // mas NÃO retorne o erro para o cliente, pois o agendamento JÁ FOI FEITO.
                Console.WriteLine($"Erro ao disparar e-mail de agendamento: {ex.Message}");
            }

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
            // 1. Busca o agendamento E os dados necessários para o e-mail
            var appointment = await _context.Appointments
                .Include(a => a.Customer)      // <-- Inclui o Cliente
                .Include(a => a.Barber)        // <-- Inclui o Barbeiro
                    .ThenInclude(b => b.UserAccount) // <-- Inclui a conta do Barbeiro
                .Include(a => a.Services)      // <-- Inclui os Serviços
                .FirstOrDefaultAsync(a => a.Id == appointmentId); // Busca pelo ID

            if (appointment == null || appointment.CustomerId != customerId)
            {
                return false; // Não encontrado ou não pertence ao cliente
            }

            // 2. Verifica se já está cancelado (para não enviar e-mail de novo)
            if (appointment.Status == AppointmentStatus.CancelledByCustomer)
            {
                return true; // Já estava cancelado
            }

            // 3. Altera o status
            appointment.Status = AppointmentStatus.CancelledByCustomer;
            await _context.SaveChangesAsync(); // Salva a mudança no banco

            // 4. Dispara o e-mail de cancelamento (Fire-and-Forget)
            try
            {
                // Os dados já foram carregados (Cliente e Barbeiro)
                await _emailService.EnviarEmailCancelamento(appointment, appointment.Customer, appointment.Barber);
            }
            catch (Exception ex)
            {
                // Loga o erro, mas não falha a operação, pois o cancelamento já ocorreu.
                Console.WriteLine($"Erro ao disparar e-mail de cancelamento: {ex.Message}");
            }

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