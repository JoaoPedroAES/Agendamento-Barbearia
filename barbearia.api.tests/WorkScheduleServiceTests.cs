using Xunit;
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
    public class WorkScheduleServiceTests
    {
        private readonly AppDbContext _context;
        private readonly IWorkScheduleService _service;
        private const int BARBER_ID = 1;

        // --- SETUP (Construtor) ---
        public WorkScheduleServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            
            _context = new AppDbContext(options);

            // Instancia o serviço
            _service = new WorkScheduleService(_context);
        }

        // --- MÉTODO HELPER (Auxiliar) ---
        // Apenas semeia o banco com um barbeiro
        private async Task SeedBarberAsync()
        {
            var user = new ApplicationUser { Id = "barberUser1", FullName = "Barbeiro Teste", Email = "barber@test.com", UserName = "barber@test.com" };
            var barber = new Barber { Id = BARBER_ID, ApplicationUserId = "barberUser1", UserAccount = user, IsActive = true };
            
            _context.Users.Add(user);
            _context.Barbers.Add(barber);
            await _context.SaveChangesAsync();
        }

        // --- TESTE 1: GetScheduleByBarberIdAsync (Caminho Feliz) ---
        [Fact]
        public async Task GetScheduleByBarberIdAsync_DeveRetornarHorariosCorretosDoBarbeiro()
        {
            // 1. Arrange
            await SeedBarberAsync();
            _context.WorkSchedules.AddRange(
                new WorkSchedule { BarberId = BARBER_ID, DayOfWeek = DayOfWeek.Monday, StartTime = TimeSpan.Parse("09:00"), EndTime = TimeSpan.Parse("18:00") },
                new WorkSchedule { BarberId = BARBER_ID, DayOfWeek = DayOfWeek.Tuesday, StartTime = TimeSpan.Parse("09:00"), EndTime = TimeSpan.Parse("18:00") },
                new WorkSchedule { BarberId = 2, DayOfWeek = DayOfWeek.Wednesday, StartTime = TimeSpan.Parse("10:00"), EndTime = TimeSpan.Parse("19:00") } // Outro barbeiro
            );
            await _context.SaveChangesAsync();

            // 2. Act
            var resultado = await _service.GetScheduleByBarberIdAsync(BARBER_ID);

            // 3. Assert
            Assert.NotNull(resultado);
            Assert.Equal(2, resultado.Count()); // Deve retornar apenas os 2 do BARBER_ID = 1
            Assert.Contains(resultado, s => s.DayOfWeek == DayOfWeek.Monday);
            Assert.DoesNotContain(resultado, s => s.DayOfWeek == DayOfWeek.Wednesday);
        }

        // --- TESTE 2: SetBatchWorkScheduleAsync (Caminho: ADICIONAR) ---
        [Fact]
        public async Task SetBatchWorkScheduleAsync_DeveAdicionarNovosHorarios()
        {
            // 1. Arrange
            await SeedBarberAsync(); // Cria o Barbeiro 1
            var dtoList = new List<SetWorkScheduleDto>
            {
                new SetWorkScheduleDto { BarberId = BARBER_ID, DayOfWeek = DayOfWeek.Monday, StartTime = TimeSpan.Parse("08:00"), EndTime = TimeSpan.Parse("17:00"), BreakStartTime = TimeSpan.Parse("12:00"), BreakEndTime = TimeSpan.Parse("13:00") }
            };
            
            // Garante que o banco está vazio
            Assert.Equal(0, await _context.WorkSchedules.CountAsync());

            // 2. Act
            await _service.SetBatchWorkScheduleAsync(dtoList);

            // 3. Assert
            Assert.Equal(1, await _context.WorkSchedules.CountAsync()); // Verifica se 1 foi adicionado
            var schedule = await _context.WorkSchedules.FirstAsync();
            Assert.Equal(DayOfWeek.Monday, schedule.DayOfWeek);
            Assert.Equal(TimeSpan.Parse("08:00"), schedule.StartTime);
        }

        // --- TESTE 3: SetBatchWorkScheduleAsync (Caminho: ATUALIZAR) ---
        [Fact]
        public async Task SetBatchWorkScheduleAsync_DeveAtualizarHorariosExistentes()
        {
            // 1. Arrange
            await SeedBarberAsync();
            // Adiciona um horário antigo para Segunda-feira
            _context.WorkSchedules.Add(new WorkSchedule { BarberId = BARBER_ID, DayOfWeek = DayOfWeek.Monday, StartTime = TimeSpan.Parse("09:00"), EndTime = TimeSpan.Parse("18:00") });
            await _context.SaveChangesAsync();

            // O novo DTO envia um horário ATUALIZADO para Segunda-feira
            var dtoList = new List<SetWorkScheduleDto>
            {
                new SetWorkScheduleDto { BarberId = BARBER_ID, DayOfWeek = DayOfWeek.Monday, StartTime = TimeSpan.Parse("10:00"), EndTime = TimeSpan.Parse("19:00"), BreakStartTime = TimeSpan.Parse("13:00"), BreakEndTime = TimeSpan.Parse("14:00") }
            };
            
            // 2. Act
            await _service.SetBatchWorkScheduleAsync(dtoList);

            // 3. Assert
            Assert.Equal(1, await _context.WorkSchedules.CountAsync()); // Ainda deve ter só 1
            var schedule = await _context.WorkSchedules.FirstAsync();
            Assert.Equal(TimeSpan.Parse("10:00"), schedule.StartTime); // Verifica se atualizou o StartTime
            Assert.Equal(TimeSpan.Parse("19:00"), schedule.EndTime);   // Verifica se atualizou o EndTime
        }

        // --- TESTE 4: SetBatchWorkScheduleAsync (Caminho: REMOVER) ---
        [Fact]
        public async Task SetBatchWorkScheduleAsync_DeveRemoverHorariosAusentesNaLista()
        {
            // 1. Arrange
            await SeedBarberAsync();
            // Adiciona horários para Segunda e Terça
            _context.WorkSchedules.AddRange(
                new WorkSchedule { BarberId = BARBER_ID, DayOfWeek = DayOfWeek.Monday, StartTime = TimeSpan.Parse("09:00"), EndTime = TimeSpan.Parse("18:00") },
                new WorkSchedule { BarberId = BARBER_ID, DayOfWeek = DayOfWeek.Tuesday, StartTime = TimeSpan.Parse("09:00"), EndTime = TimeSpan.Parse("18:00") }
            );
            await _context.SaveChangesAsync();
            Assert.Equal(2, await _context.WorkSchedules.CountAsync()); // Garante que 2 existem

            // A nova lista (DTO) envia APENAS Segunda-feira (Terça-feira foi "desmarcada" no front)
            var dtoList = new List<SetWorkScheduleDto>
            {
                new SetWorkScheduleDto { BarberId = BARBER_ID, DayOfWeek = DayOfWeek.Monday, StartTime = TimeSpan.Parse("09:00"), EndTime = TimeSpan.Parse("18:00"), BreakStartTime = TimeSpan.Parse("12:00"), BreakEndTime = TimeSpan.Parse("13:00") }
            };

            // 2. Act
            await _service.SetBatchWorkScheduleAsync(dtoList);

            // 3. Assert
            Assert.Equal(1, await _context.WorkSchedules.CountAsync()); // Deve ter 1 (Terça foi removida)
            var schedule = await _context.WorkSchedules.FirstAsync();
            Assert.Equal(DayOfWeek.Monday, schedule.DayOfWeek); // Apenas Segunda sobrou
        }

        // --- TESTE 5: SetBatchWorkScheduleAsync (Caminho: Misto - Add, Update, Remove) ---
        [Fact]
        public async Task SetBatchWorkScheduleAsync_DeveLidarComAdicionarAtualizarERemoverJuntos()
        {
            // 1. Arrange
            await SeedBarberAsync();
            // Barbeiro trabalha Segunda e Terça
            _context.WorkSchedules.AddRange(
                new WorkSchedule { BarberId = BARBER_ID, DayOfWeek = DayOfWeek.Monday, StartTime = TimeSpan.Parse("09:00"), EndTime = TimeSpan.Parse("18:00") }, // Vai ser atualizado
                new WorkSchedule { BarberId = BARBER_ID, DayOfWeek = DayOfWeek.Tuesday, StartTime = TimeSpan.Parse("09:00"), EndTime = TimeSpan.Parse("18:00") } // Vai ser removido
            );
            await _context.SaveChangesAsync();

            // Nova lista: Atualiza Segunda, Remove Terça, Adiciona Quarta
            var dtoList = new List<SetWorkScheduleDto>
            {
                new SetWorkScheduleDto { BarberId = BARBER_ID, DayOfWeek = DayOfWeek.Monday, StartTime = TimeSpan.Parse("10:00"), EndTime = TimeSpan.Parse("17:00"), BreakStartTime = TimeSpan.Parse("12:00"), BreakEndTime = TimeSpan.Parse("13:00") }, // Atualiza
                new SetWorkScheduleDto { BarberId = BARBER_ID, DayOfWeek = DayOfWeek.Wednesday, StartTime = TimeSpan.Parse("08:00"), EndTime = TimeSpan.Parse("12:00"), BreakStartTime = TimeSpan.Parse("00:00"), BreakEndTime = TimeSpan.Parse("00:00") } // Adiciona
            };

            // 2. Act
            await _service.SetBatchWorkScheduleAsync(dtoList);

            // 3. Assert
            var schedulesNoBanco = await _context.WorkSchedules.ToListAsync();
            Assert.Equal(2, schedulesNoBanco.Count); // Deve ter 2 (Segunda e Quarta)
            
            var seg = schedulesNoBanco.FirstOrDefault(s => s.DayOfWeek == DayOfWeek.Monday);
            var ter = schedulesNoBanco.FirstOrDefault(s => s.DayOfWeek == DayOfWeek.Tuesday);
            var qua = schedulesNoBanco.FirstOrDefault(s => s.DayOfWeek == DayOfWeek.Wednesday);

            Assert.NotNull(seg);
            Assert.Null(ter); // Terça deve ter sido removida
            Assert.NotNull(qua);
            Assert.Equal(TimeSpan.Parse("10:00"), seg.StartTime); // Segunda foi atualizada
        }

        // --- TESTE 6: SetBatchWorkScheduleAsync (Validação) ---
        [Fact]
        public async Task SetBatchWorkScheduleAsync_DeveLancarExcecao_QuandoHorarioInvalido()
        {
            // 1. Arrange
            await SeedBarberAsync();
            var dtoList = new List<SetWorkScheduleDto>
            {
                // Horário de Início (18:00) é DEPOIS do Fim (09:00)
                new SetWorkScheduleDto { BarberId = BARBER_ID, DayOfWeek = DayOfWeek.Monday, StartTime = TimeSpan.Parse("18:00"), EndTime = TimeSpan.Parse("09:00"), BreakStartTime = TimeSpan.Parse("12:00"), BreakEndTime = TimeSpan.Parse("13:00") }
            };

            // 2. Act & 3. Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.SetBatchWorkScheduleAsync(dtoList)
            );
            
            Assert.Equal($"Horários inválidos para {DayOfWeek.Monday}.", exception.Message);
        }
    }
}