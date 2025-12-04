using Xunit;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using barbearia.api.Data;
using barbearia.api.Models;
using barbearia.api.Services;
using barbearia.api.Dtos;
using System.Collections.Generic; // <-- Adicionado para o Teste 6

namespace barbearia.api.tests
{
    public class ServicesServiceTests
    {
        private readonly AppDbContext _context;
        private readonly ServicesService _service;

        // --- SETUP (Construtor) ---
        public ServicesServiceTests()
        {
            // 1. Configurar o Banco de Dados em Memória
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString()) // BD novo para cada teste
                .Options;
            
            _context = new AppDbContext(options);

            // 2. Instanciar o Serviço
            _service = new ServicesService(_context);
        }

        // --- MÉTODO HELPER (Auxiliar) ---
        private async Task<Service> SeedDatabaseAsync()
        {
            var service = new Service
            {
                Id = 1, // Pré-definido para testes de Update/Delete
                Name = "Corte Padrão",
                Description = "Corte na máquina",
                Price = 30.00m,
                DurationInMinutes = 20
            };
            
            _context.Services.Add(service);
            await _context.SaveChangesAsync();
            return service;
        }

        // --- TESTE 1: CreateServiceAsync ---
        [Fact]
        public async Task CreateServiceAsync_DeveAdicionarServicoAoBanco()
        {
            // 1. Arrange
            var dto = new CreateServiceDto
            {
                Name = "Barba",
                Description = "Barba completa",
                Price = 25.00m,
                DurationInMinutes = 15
            };

            // 2. Act
            var resultado = await _service.CreateServiceAsync(dto);

            // 3. Assert
            Assert.NotNull(resultado);
            Assert.Equal("Barba", resultado.Name);
            Assert.Equal(25.00m, resultado.Price);

            // Verifica se foi realmente salvo no banco
            Assert.Equal(1, await _context.Services.CountAsync());
            var servicoDoBanco = await _context.Services.FirstAsync();
            Assert.Equal("Barba", servicoDoBanco.Name);
            Assert.Equal(15, servicoDoBanco.DurationInMinutes);
        }

        // --- TESTE 2: UpdateServiceAsync (Caso de Sucesso) ---
        [Fact]
        public async Task UpdateServiceAsync_DeveAtualizarServicoCorretamente_QuandoEncontrado()
        {
            // 1. Arrange
            await SeedDatabaseAsync(); // Cria o serviço com Id 1
            var dto = new UpdateServiceDto
            {
                Name = "Corte Atualizado",
                Description = "Descrição Nova",
                Price = 40.00m,
                DurationInMinutes = 30
            };

            // 2. Act
            var resultado = await _service.UpdateServiceAsync(1, dto);

            // 3. Assert
            Assert.NotNull(resultado);
            Assert.Equal(40.00m, resultado.Price);
            Assert.Equal("Corte Atualizado", resultado.Name);

            // Verifica se foi salvo no banco
            var servicoDoBanco = await _context.Services.FindAsync(1);
            Assert.Equal("Corte Atualizado", servicoDoBanco.Name);
            Assert.Equal("Descrição Nova", servicoDoBanco.Description);
        }

        // --- TESTE 3: UpdateServiceAsync (Caso de Falha) ---
        [Fact]
        public async Task UpdateServiceAsync_DeveRetornarNull_QuandoServicoNaoEncontrado()
        {
            // 1. Arrange
            var dto = new UpdateServiceDto { Name = "Corte Fantasma" };
            // Banco está vazio

            // 2. Act
            var resultado = await _service.UpdateServiceAsync(99, dto); // ID 99 não existe

            // 3. Assert
            Assert.Null(resultado);
        }

        // --- TESTE 4: DeleteServiceAsync (Caso de Sucesso) ---
        [Fact]
        public async Task DeleteServiceAsync_DeveRemoverServico_QuandoEncontrado()
        {
            // 1. Arrange
            await SeedDatabaseAsync(); // Cria o serviço com Id 1
            Assert.Equal(1, await _context.Services.CountAsync()); // Garante que o item está lá

            // 2. Act
            var resultado = await _service.DeleteServiceAsync(1);

            // 3. Assert
            Assert.True(resultado);
            // Verifica se foi realmente removido do banco
            Assert.Equal(0, await _context.Services.CountAsync());
        }

        // --- TESTE 5: DeleteServiceAsync (Caso de Falha) ---
        [Fact]
        public async Task DeleteServiceAsync_DeveRetornarFalse_QuandoNaoEncontrado()
        {
            // 1. Arrange
            // Banco está vazio

            // 2. Act
            var resultado = await _service.DeleteServiceAsync(99); // ID 99 não existe

            // 3. Assert
            Assert.False(resultado);
        }

        // --- ▼▼▼ NOVOS TESTES ADICIONADOS ABAIXO ▼▼▼ ---

        // --- TESTE 6: GetAllServicesAsync (Caminho Feliz) ---
        [Fact]
        public async Task GetAllServicesAsync_DeveRetornarTodosOsServicos()
        {
            // 1. Arrange
            // Adiciona dois serviços ao banco
            _context.Services.AddRange(
                new Service { Name = "Corte", Description = "Desc1", Price = 30, DurationInMinutes = 30 },
                new Service { Name = "Barba", Description = "Desc2", Price = 20, DurationInMinutes = 20 }
            );
            await _context.SaveChangesAsync();

            // 2. Act
            var resultado = await _service.GetAllServicesAsync();

            // 3. Assert
            Assert.NotNull(resultado);
            Assert.Equal(2, resultado.Count()); // Verifica se retornou 2 serviços
            Assert.Contains(resultado, s => s.Name == "Corte");
        }

        // --- TESTE 7: GetServiceByIdAsync (Caminho Feliz) ---
        [Fact]
        public async Task GetServiceByIdAsync_DeveRetornarServicoCorreto_QuandoEncontrado()
        {
            // 1. Arrange
            await SeedDatabaseAsync(); // Cria o serviço com Id 1 (Name = "Corte Padrão")
            
            // 2. Act
            var resultado = await _service.GetServiceByIdAsync(1);

            // 3. Assert
            Assert.NotNull(resultado);
            Assert.Equal(1, resultado.Id);
            Assert.Equal("Corte Padrão", resultado.Name);
        }

        // --- TESTE 8: GetServiceByIdAsync (Caso de Falha) ---
        [Fact]
        public async Task GetServiceByIdAsync_DeveRetornarNull_QuandoNaoEncontrado()
        {
            // 1. Arrange
            // Banco está vazio

            // 2. Act
            var resultado = await _service.GetServiceByIdAsync(99); // ID 99 não existe

            // 3. Assert
            Assert.Null(resultado);
        }
    }
}