using barbearia.api.Models;

namespace barbearia.api.Services
{
    public interface IEmailService
    {
        // Envia um e-mail de confirmação de agendamento para o cliente e o barbeiro
        Task EnviarEmailConfirmacaoAgendamento(Appointment agendamento, ApplicationUser cliente, Barber barbeiro);

        // Envia um e-mail de cancelamento de agendamento para o cliente e o barbeiro
        Task EnviarEmailCancelamento(Appointment agendamento, ApplicationUser cliente, Barber barbeiro);
    }
}
