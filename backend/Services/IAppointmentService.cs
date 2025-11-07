using barbearia.api.Dtos;
using barbearia.api.Models;

namespace barbearia.api.Services
{
    public interface IAppointmentService
    {
        // Cria um novo agendamento, retornando o agendamento criado ou lançando exceções
        Task<Appointment> CreateAppointmentAsync(CreateAppointmentDto dto, string customerId);

        // Busca os agendamentos de um cliente específico
        Task<IEnumerable<Appointment>> GetMyAppointmentsAsync(string customerId);

        // Cancela um agendamento (retorna true se sucesso, false se não encontrado/permitido)
        Task<bool> CancelAppointmentAsync(int appointmentId, string customerId);

        // Busca a agenda para uma data (para Admin/Barbeiro)
        Task<IEnumerable<Appointment>> GetAgendaAsync(DateTime date, string userId, bool isBarber);
    }
}
