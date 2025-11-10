using barbearia.api.Models;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace barbearia.api.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task EnviarEmailConfirmacaoAgendamento(Appointment agendamento, ApplicationUser cliente, Barber barbeiro)
        {
            //Pega a API Key (que está salva no user-secrets)
            var apiKey = _configuration["SendGridApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                //Lidar com o erro
                Console.WriteLine("API Key do SendGrid não encontrada.");
                return;
            }
            var client = new SendGridClient(apiKey);

            //Define o remetente
            var from = new EmailAddress("jparaujo1234@hotmail.com", "Barbearia Top");

            //Define o Assunto
            var subject = $"Confirmação de Agendamento - {agendamento.StartDateTime:dd/MM/yyyy HH:mm}";

            //Define os destinatários
            var toCliente = new EmailAddress(cliente.Email, cliente.FullName);
            var toBarbeiro = new EmailAddress(barbeiro.UserAccount.Email, barbeiro.UserAccount.FullName); // Assumindo que o e-mail está no UserAccount

            //Cria o conteúdo
            var htmlContentCliente = $@"
                <h1>Olá, {cliente.FullName}!</h1>
                <p>Seu agendamento na Barbearia Top foi confirmado com sucesso.</p>
                <p><strong>Barbeiro:</strong> {barbeiro.UserAccount.FullName}</p>
                <p><strong>Data:</strong> {agendamento.StartDateTime:dd/MM/yyyy}</p>
                <p><strong>Hora:</strong> {agendamento.StartDateTime:HH:mm}</p>
                <p><strong>Serviços:</strong> {string.Join(", ", agendamento.Services.Select(s => s.Name))}</p>
                <p>Obrigado por agendar conosco!</p>";

            var htmlContentBarbeiro = $@"
                <h1>Novo Agendamento Recebido!</h1>
                <p>Você tem um novo agendamento, {barbeiro.UserAccount.FullName}.</p>
                <p><strong>Cliente:</strong> {cliente.FullName}</p>
                <p><strong>Data:</strong> {agendamento.StartDateTime:dd/MM/yyyy}</p>
                <p><strong>Hora:</strong> {agendamento.StartDateTime:HH:mm}</p>
                <p><strong>Serviços:</strong> {string.Join(", ", agendamento.Services.Select(s => s.Name))}</p>";

            //Envia o e-mail para o Cliente
            var msgCliente = MailHelper.CreateSingleEmail(from, toCliente, subject, "", htmlContentCliente);
            var responseCliente = await client.SendEmailAsync(msgCliente);

            //Envia o e-mail para o Barbeiro
            var msgBarbeiro = MailHelper.CreateSingleEmail(from, toBarbeiro, subject, "", htmlContentBarbeiro);
            var responseBarbeiro = await client.SendEmailAsync(msgBarbeiro);

            // (Opcional) Verificar se deu certo (response.IsSuccessStatusCode)     
        }

        public async Task EnviarEmailCancelamento(Appointment agendamento, ApplicationUser cliente, Barber barbeiro)
        {
            var apiKey = _configuration["SendGridApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                Console.WriteLine("API Key do SendGrid não encontrada.");
                return;
            }
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("jparaujo1234@hotmail.com", "Barbearia Top"); // Lembre-se que você trocou este e-mail
            var subject = $"Agendamento Cancelado - {agendamento.StartDateTime:dd/MM/yyyy HH:mm}";

            // Destinatário do Cliente (Sabemos que este funciona)
            var toCliente = new EmailAddress(cliente.Email, cliente.FullName);

            // Destinatário do Barbeiro (Suspeito)
            var toBarbeiro = (barbeiro?.UserAccount?.Email != null)
                ? new EmailAddress(barbeiro.UserAccount.Email, barbeiro.UserAccount.FullName)
                : null; // Será nulo se o e-mail não for encontrado

            // Conteúdo do Cliente
            var htmlContentCliente = $@"
                <h1>Olá, {cliente.FullName}.</h1>
                <p>Seu agendamento na Barbearia Top foi <strong>cancelado</strong> com sucesso.</p>
                <p><strong>Detalhes do agendamento cancelado:</strong></p>
                <p><strong>Barbeiro:</strong> {barbeiro?.UserAccount?.FullName ?? "N/A"}</p>
                <p><strong>Data:</strong> {agendamento.StartDateTime:dd/MM/yyyy}</p>
                <p><strong>Hora:</strong> {agendamento.StartDateTime:HH:mm}</p>
                <p>Esperamos vê-lo em breve.</p>";

            var htmlContentBarbeiro = $@"
                <h1>Agendamento Cancelado</h1>
                <p>Um agendamento foi cancelado por um cliente, {barbeiro?.UserAccount?.FullName ?? "Barbeiro"}.</p>
                <p><strong>Cliente:</strong> {cliente.FullName}</p>
                <p><strong>Data:</strong> {agendamento.StartDateTime:dd/MM/yyyy}</p>
                <p><strong>Hora:</strong> {agendamento.StartDateTime:HH:mm}</p>
                <p><strong>Serviços:</strong> {string.Join(", ", agendamento.Services.Select(s => s.Name))}</p>
                <p>Este horário agora está livre em sua agenda.</p>";

            // 5. Envia o e-mail para o Cliente
            var msgCliente = MailHelper.CreateSingleEmail(from, toCliente, subject, "", htmlContentCliente);
            await client.SendEmailAsync(msgCliente);

            // --- 6. ENVIO PARA O BARBEIRO (COM VERIFICAÇÃO) ---
            if (toBarbeiro != null)
            {
                var msgBarbeiro = MailHelper.CreateSingleEmail(from, toBarbeiro, subject, "", htmlContentBarbeiro);
                var responseBarbeiro = await client.SendEmailAsync(msgBarbeiro);

                // Se falhou, vamos imprimir o motivo no console da API
                if (!responseBarbeiro.IsSuccessStatusCode)
                {
                    Console.WriteLine($"ERRO: Falha ao enviar e-mail de cancelamento PARA O BARBEIRO: {toBarbeiro.Email}");
                    string responseBody = await responseBarbeiro.Body.ReadAsStringAsync();
                    Console.WriteLine($"RESPOSTA DO SENDGRID: {responseBody}");
                }
            }
            else
            {
                // Se o e-mail era nulo, vamos logar isso
                Console.WriteLine("AVISO: E-mail de cancelamento para o Barbeiro IGNORADO (e-mail nulo ou barbeiro/usuário não encontrado na consulta).");
            }
        }
    }
}
