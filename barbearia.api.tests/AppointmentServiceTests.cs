using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using barbearia.api.Data;
using barbearia.api.Models;
using barbearia.api.Services;
using barbearia.api.Dtos;
using System;
using System.Collections.Generic;

namespace barbearia.api.tests
{
    public class AppointmentServiceTests
    {
        private readonly AppDbContext _context;
        private readonly Mock<IAvailabilityService> _mockAvailabilityService;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly AppointmentService _service;

        // --- IDs Constantes para Teste ---
        private const int BARBER_ID = 1;
        private const int SERVICE_ID = 1;
        private const string CUSTOMER_ID = "customer1";

        // --- SETUP (Construtor) ---
        public AppointmentServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            
            _context = new AppDbContext(options);

            _mockAvailabilityService = new Mock<IAvailabilityService>();
            _mockEmailService = new Mock<IEmailService>();

            _service = new AppointmentService(_context, _mockAvailabilityService.Object, _mockEmailService.Object);
        }

        // --- MÉTODO HELPER (Auxiliar) ---
        private async Task SeedDatabaseAsync()
        {
            var userCliente = new ApplicationUser { Id = CUSTOMER_ID, FullName = "Cliente Teste", Email = "cliente@teste.com", UserName = "cliente@teste.com" };
            var userBarbeiro = new ApplicationUser { Id = "barberUser1", FullName = "Barbeiro Teste", Email = "barbeiro@email.com", UserName = "barbeiro@email.com" };
            var barber = new Barber { Id = BARBER_ID, ApplicationUserId = "barberUser1", UserAccount = userBarbeiro };
            var service = new Service { Id = SERVICE_ID, Name = "Corte", Description = "Desc", Price = 50, DurationInMinutes = 30 };

            _context.Users.AddRange(userCliente, userBarbeiro);
            _context.Barbers.Add(barber);
            _context.Services.Add(service);
            await _context.SaveChangesAsync();
        }


        // --- TESTE 1: CreateAppointmentAsync (Caminho Feliz) ---
        [Fact]
        public async Task CreateAppointmentAsync_DeveCriarAgendamento_QuandoSlotEstaDisponivel()
        {
            // 1. Arrange
            await SeedDatabaseAsync();
            var dataHora = new DateTime(2025, 11, 20, 10, 0, 0, DateTimeKind.Utc);
            var dto = new CreateAppointmentDto
            {
                BarberId = BARBER_ID,
                ServiceIds = new List<int> { SERVICE_ID },
                StartDateTime = dataHora
            };
            
            var expectedEndTime = dataHora.AddMinutes(30); 

            // Simula o AvailabilityService
            var slotsLivres = new List<TimeSpan> { TimeSpan.Parse("10:00") };
            _mockAvailabilityService.Setup(s => s.GetAvailableSlotsAsync(BARBER_ID, dto.ServiceIds, dataHora.Date))
                                    .ReturnsAsync(slotsLivres);

            // Simula o EmailService
            _mockEmailService.Setup(s => s.EnviarEmailConfirmacaoAgendamento(
                                        It.IsAny<Appointment>(), 
                                        It.IsAny<ApplicationUser>(), 
                                        It.IsAny<Barber>()))
                                    .Returns(Task.CompletedTask);

            // 2. Act
            var resultado = await _service.CreateAppointmentAsync(dto, CUSTOMER_ID);

            // 3. Assert
            Assert.NotNull(resultado);
            Assert.Equal(BARBER_ID, resultado.BarberId);
            Assert.Equal(CUSTOMER_ID, resultado.CustomerId);
            Assert.Equal(50, resultado.TotalPrice); 
            Assert.Equal(expectedEndTime, resultado.EndDateTime); 

            Assert.Equal(1, await _context.Appointments.CountAsync());
            _mockEmailService.Verify(s => s.EnviarEmailConfirmacaoAgendamento(
                                        It.IsAny<Appointment>(), 
                                        It.IsAny<ApplicationUser>(), 
                                        It.IsAny<Barber>()), Times.Once); 
        }

        // --- TESTE 2: CreateAppointmentAsync (Slot Ocupado) ---
        [Fact]
        public async Task CreateAppointmentAsync_DeveLancarExcecao_QuandoSlotNaoEstaDisponivel()
        {
            // 1. Arrange
            await SeedDatabaseAsync();
            var dataHora = new DateTime(2025, 11, 20, 10, 0, 0, DateTimeKind.Utc);
            var dto = new CreateAppointmentDto
            {
                BarberId = BARBER_ID,
                ServiceIds = new List<int> { SERVICE_ID },
                StartDateTime = dataHora
            };

            var slotsLivres = new List<TimeSpan> { TimeSpan.Parse("09:00"), TimeSpan.Parse("09:30") }; 
            _mockAvailabilityService.Setup(s => s.GetAvailableSlotsAsync(BARBER_ID, dto.ServiceIds, dataHora.Date))
                                    .ReturnsAsync(slotsLivres);

            // 2. Act & 3. Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.CreateAppointmentAsync(dto, CUSTOMER_ID)
            );
            Assert.Equal("O horário selecionado não está mais disponível.", exception.Message);
            Assert.Equal(0, await _context.Appointments.CountAsync());
        }

        // --- TESTE 3: CancelAppointmentAsync (Caminho Feliz) ---
        [Fact]
        public async Task CancelAppointmentAsync_DeveMudarStatusParaCancelado_QuandoEncontrado()
        {
            // 1. Arrange
            await SeedDatabaseAsync();
            var dataHora = new DateTime(2025, 11, 20, 10, 0, 0, DateTimeKind.Utc);
            var agendamento = new Appointment
            {
                Id = 1,
                CustomerId = CUSTOMER_ID,
                BarberId = BARBER_ID,
                StartDateTime = dataHora,
                EndDateTime = dataHora.AddMinutes(30),
                Status = AppointmentStatus.Scheduled,
                Barber = _context.Barbers.Find(BARBER_ID),       
                Customer = _context.Users.Find(CUSTOMER_ID), 
                Services = new List<Service> { _context.Services.Find(SERVICE_ID) } 
            };
            _context.Appointments.Add(agendamento);
            await _context.SaveChangesAsync();
            
             _mockEmailService.Setup(s => s.EnviarEmailCancelamento(
                                        It.IsAny<Appointment>(), 
                                        It.IsAny<ApplicationUser>(), 
                                        It.IsAny<Barber>()))
                                    .Returns(Task.CompletedTask);

            // 2. Act
            var resultado = await _service.CancelAppointmentAsync(1, CUSTOMER_ID);

            // 3. Assert
            Assert.True(resultado); 
            var agendamentoDoBanco = await _context.Appointments.FindAsync(1);
            Assert.Equal(AppointmentStatus.CancelledByCustomer, agendamentoDoBanco.Status);
            _mockEmailService.Verify(s => s.EnviarEmailCancelamento(
                                        It.IsAny<Appointment>(), 
                                        It.IsAny<ApplicationUser>(), 
                                        It.IsAny<Barber>()), Times.Once);
        }

        // --- TESTE 4: CancelAppointmentAsync (Caso de Falha) ---
        [Fact]
        public async Task CancelAppointmentAsync_DeveRetornarFalse_QuandoNaoPertenceAoCliente()
        {
            // 1. Arrange
            await SeedDatabaseAsync();
            var agendamento = new Appointment
            {
                Id = 1,
                CustomerId = "ID_DE_OUTRO_CLIENTE", 
                BarberId = BARBER_ID,
                Status = AppointmentStatus.Scheduled,
                Barber = _context.Barbers.Find(BARBER_ID),       
                Customer = new ApplicationUser { 
                    Id = "ID_DE_OUTRO_CLIENTE", 
                    Email = "outro@teste.com", 
                    UserName = "outro@teste.com", 
                    FullName = "Outro Cliente" // <-- Correção
                },
                Services = new List<Service> { _context.Services.Find(SERVICE_ID) } 
            };
            _context.Appointments.Add(agendamento);
            await _context.SaveChangesAsync(); 

            // 2. Act
            var resultado = await _service.CancelAppointmentAsync(1, CUSTOMER_ID);

            // 3. Assert
            Assert.False(resultado); 
            var agendamentoDoBanco = await _context.Appointments.FindAsync(1);
            Assert.Equal(AppointmentStatus.Scheduled, agendamentoDoBanco.Status);
            _mockEmailService.Verify(s => s.EnviarEmailCancelamento(
                                        It.IsAny<Appointment>(), 
                                        It.IsAny<ApplicationUser>(), 
                                        It.IsAny<Barber>()), Times.Never);
        }

        // --- ▼▼▼ TESTES NOVOS ADICIONADOS ▼▼▼ ---

        // --- TESTE 5: GetMyAppointmentsAsync (Caminho Feliz) ---
        [Fact]
        public async Task GetMyAppointmentsAsync_DeveRetornarApenasAgendamentosDoClienteCorreto()
        {
            // 1. Arrange
            await SeedDatabaseAsync(); // Cria customer1, barber1, service1
            
            // Cria um segundo cliente e agendamento
            var customer2 = new ApplicationUser { Id = "customer2", FullName = "Cliente Fantasma", Email = "fantasma@teste.com", UserName = "fantasma@teste.com" };
            _context.Users.Add(customer2);

            _context.Appointments.AddRange(
                new Appointment { Id = 1, CustomerId = CUSTOMER_ID, BarberId = BARBER_ID, Status = AppointmentStatus.Scheduled, StartDateTime = DateTime.UtcNow.AddDays(1) },
                new Appointment { Id = 2, CustomerId = "customer2", BarberId = BARBER_ID, Status = AppointmentStatus.Scheduled, StartDateTime = DateTime.UtcNow.AddDays(2) }
            );
            await _context.SaveChangesAsync();

            // 2. Act
            // Busca agendamentos apenas do CUSTOMER_ID (o "Cliente Teste")
            var resultado = await _service.GetMyAppointmentsAsync(CUSTOMER_ID);

            // 3. Assert
            Assert.NotNull(resultado);
            Assert.Single(resultado); // Deve retornar APENAS 1 agendamento
            Assert.Equal(1, resultado.First().Id); // Deve ser o agendamento de Id 1
            Assert.Equal(CUSTOMER_ID, resultado.First().CustomerId);
        }

        // --- TESTE 6: GetMyAppointmentsAsync (Caminho Vazio) ---
        [Fact]
        public async Task GetMyAppointmentsAsync_DeveRetornarListaVazia_QuandoClienteNaoTemAgendamentos()
        {
            // 1. Arrange
            await SeedDatabaseAsync(); // Cria o cliente, mas nenhum agendamento
            
            // 2. Act
            var resultado = await _service.GetMyAppointmentsAsync(CUSTOMER_ID);

            // 3. Assert
            Assert.NotNull(resultado);
            Assert.Empty(resultado); // A lista deve estar vazia
        }
    }
}