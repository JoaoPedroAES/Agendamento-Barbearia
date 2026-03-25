using barbearia.api.Models;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace barbearia.api.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        // Construtor que recebe a configuração para acessar a API Key do SendGrid
        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Método para enviar e-mail de confirmação de agendamento
        public async Task EnviarEmailConfirmacaoAgendamento(Appointment agendamento, ApplicationUser cliente, Barber barbeiro)
        {
            // Obtém a API Key do SendGrid
            var apiKey = _configuration["SendGridApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                // Loga um erro se a API Key não for encontrada
                Console.WriteLine("API Key do SendGrid não encontrada.");
                return;
            }
            var client = new SendGridClient(apiKey);

            // Define o remetente do e-mail
            var from = new EmailAddress("barbershop.pfc@gmail.com", "BarberShop");

            // Define o assunto do e-mail
            var subject = $"Confirmação de Agendamento - {agendamento.StartDateTime:dd/MM/yyyy HH:mm}";

            // Define os destinatários (cliente e barbeiro)
            var toCliente = new EmailAddress(cliente.Email, cliente.FullName);
            var toBarbeiro = new EmailAddress(barbeiro.UserAccount.Email, barbeiro.UserAccount.FullName);

            // Cria o conteúdo do e-mail para o cliente
            var htmlContentCliente = $@"
                <h1>Olá, {cliente.FullName}!</h1>
                <p>Seu agendamento na BarberShop foi confirmado com sucesso.</p>
                <p><strong>Barbeiro:</strong> {barbeiro.UserAccount.FullName}</p>
                <p><strong>Data:</strong> {agendamento.StartDateTime:dd/MM/yyyy}</p>
                <p><strong>Hora:</strong> {agendamento.StartDateTime:HH:mm}</p>
                <p><strong>Serviços:</strong> {string.Join(", ", agendamento.Services.Select(s => s.Name))}</p>
                <p>Obrigado por agendar conosco!</p>";

            // Cria o conteúdo do e-mail para o barbeiro
            var htmlContentBarbeiro = $@"
                <h1>Novo Agendamento Recebido!</h1>
                <p>Você tem um novo agendamento, {barbeiro.UserAccount.FullName}.</p>
                <p><strong>Cliente:</strong> {cliente.FullName}</p>
                <p><strong>Data:</strong> {agendamento.StartDateTime:dd/MM/yyyy}</p>
                <p><strong>Hora:</strong> {agendamento.StartDateTime:HH:mm}</p>
                <p><strong>Serviços:</strong> {string.Join(", ", agendamento.Services.Select(s => s.Name))}</p>";

            // Envia o e-mail para o cliente
            var msgCliente = MailHelper.CreateSingleEmail(from, toCliente, subject, "", htmlContentCliente);
            var responseCliente = await client.SendEmailAsync(msgCliente);

            // Envia o e-mail para o barbeiro
            var msgBarbeiro = MailHelper.CreateSingleEmail(from, toBarbeiro, subject, "", htmlContentBarbeiro);
            var responseBarbeiro = await client.SendEmailAsync(msgBarbeiro);

            // (Opcional) Verificar se o envio foi bem-sucedido
        }

        // Método para enviar e-mail de cancelamento de agendamento
        public async Task EnviarEmailCancelamento(Appointment agendamento, ApplicationUser cliente, Barber barbeiro)
        {
            // Obtém a API Key do SendGrid
            var apiKey = _configuration["SendGridApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                // Loga um erro se a API Key não for encontrada
                Console.WriteLine("API Key do SendGrid não encontrada.");
                return;
            }
            var client = new SendGridClient(apiKey);

            // Define o remetente do e-mail
            var from = new EmailAddress("barbershop.pfc@gmail.com", "BarberShop");

            // Define o assunto do e-mail
            var subject = $"Agendamento Cancelado - {agendamento.StartDateTime:dd/MM/yyyy HH:mm}";

            // Define o destinatário do cliente
            var toCliente = new EmailAddress(cliente.Email, cliente.FullName);

            // Define o destinatário do barbeiro (se disponível)
            var toBarbeiro = (barbeiro?.UserAccount?.Email != null)
                ? new EmailAddress(barbeiro.UserAccount.Email, barbeiro.UserAccount.FullName)
                : null;

            // Cria o conteúdo do e-mail para o cliente
            var htmlContentCliente = $@"
                <h1>Olá, {cliente.FullName}.</h1>
                <p>Seu agendamento na BarberShop foi <strong>cancelado</strong> com sucesso.</p>
                <p><strong>Detalhes do agendamento cancelado:</strong></p>
                <p><strong>Barbeiro:</strong> {barbeiro?.UserAccount?.FullName ?? "N/A"}</p>
                <p><strong>Data:</strong> {agendamento.StartDateTime:dd/MM/yyyy}</p>
                <p><strong>Hora:</strong> {agendamento.StartDateTime:HH:mm}</p>
                <p>Esperamos vê-lo em breve.</p>";

            // Cria o conteúdo do e-mail para o barbeiro
            var htmlContentBarbeiro = $@"
                <h1>Agendamento Cancelado</h1>
                <p>Um agendamento foi cancelado por um cliente, {barbeiro?.UserAccount?.FullName ?? "Barbeiro"}.</p>
                <p><strong>Cliente:</strong> {cliente.FullName}</p>
                <p><strong>Data:</strong> {agendamento.StartDateTime:dd/MM/yyyy}</p>
                <p><strong>Hora:</strong> {agendamento.StartDateTime:HH:mm}</p>
                <p><strong>Serviços:</strong> {string.Join(", ", agendamento.Services.Select(s => s.Name))}</p>
                <p>Este horário agora está livre em sua agenda.</p>";

            // Envia o e-mail para o cliente
            var msgCliente = MailHelper.CreateSingleEmail(from, toCliente, subject, "", htmlContentCliente);
            await client.SendEmailAsync(msgCliente);

            // Envia o e-mail para o barbeiro (se o e-mail for válido)
            if (toBarbeiro != null)
            {
                var msgBarbeiro = MailHelper.CreateSingleEmail(from, toBarbeiro, subject, "", htmlContentBarbeiro);
                var responseBarbeiro = await client.SendEmailAsync(msgBarbeiro);

                // Loga um erro se o envio para o barbeiro falhar
                if (!responseBarbeiro.IsSuccessStatusCode)
                {
                    Console.WriteLine($"ERRO: Falha ao enviar e-mail de cancelamento PARA O BARBEIRO: {toBarbeiro.Email}");
                    string responseBody = await responseBarbeiro.Body.ReadAsStringAsync();
                    Console.WriteLine($"RESPOSTA DO SENDGRID: {responseBody}");
                }
            }
            else
            {
                // Loga um aviso se o e-mail do barbeiro for nulo
                Console.WriteLine("AVISO: E-mail de cancelamento para o Barbeiro IGNORADO (e-mail nulo ou barbeiro/usuário não encontrado na consulta).");
            }
        }
    }
}
