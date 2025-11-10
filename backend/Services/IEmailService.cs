using barbearia.api.Models;

namespace barbearia.api.Services
{
    public interface IEmailService
    {
        Task EnviarEmailConfirmacaoAgendamento(Appointment agendamento, ApplicationUser cliente, Barber barbeiro);
        Task EnviarEmailCancelamento(Appointment agendamento, ApplicationUser cliente, Barber barbeiro);
    }
}
