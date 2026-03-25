using Xunit;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using barbearia.api.Data;
using barbearia.api.Models;
using barbearia.api.Services;
using barbearia.api.Dtos; 
using System;

namespace barbearia.api.tests
{
    public class BarberServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly AppDbContext _context;
        private readonly BarberService _service;

        // --- SETUP (Construtor) ---
        public BarberServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString()) 
                .Options;
            
            _context = new AppDbContext(options);

            var store = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);

            _service = new BarberService(_mockUserManager.Object, _context);
        }

        // --- MÉTODO HELPER (Auxiliar) ---
        private async Task SeedDatabaseAsync()
        {
            var user = new ApplicationUser { Id = "user1", FullName = "Barbeiro Teste", Email = "teste@teste.com", UserName = "teste@teste.com" };
            var barber = new Barber 
            { 
                Id = 1, 
                ApplicationUserId = "user1", 
                UserAccount = user, 
                IsActive = true, 
                HasAcceptedTerms = false, 
                Bio = "Bio Antiga" 
            };
            
            _context.Users.Add(user);
            _context.Barbers.Add(barber);
            await _context.SaveChangesAsync();
        }

        // --- TESTE 1 ---
        [Fact]
        public async Task GetActiveBarbersAsync_DeveRetornarApenasBarbeirosAtivos()
        {
            // 1. ARRANGE
            var userAtivo = new ApplicationUser { Id = "user1", FullName = "Barbeiro Ativo" };
            var userInativo = new ApplicationUser { Id = "user2", FullName = "Barbeiro Inativo" };
            var barbeiroAtivo = new Barber { Id = 1, ApplicationUserId = "user1", UserAccount = userAtivo, IsActive = true };
            var barbeiroInativo = new Barber { Id = 2, ApplicationUserId = "user2", UserAccount = userInativo, IsActive = false };

            _context.Users.AddRange(userAtivo, userInativo);
            _context.Barbers.AddRange(barbeiroAtivo, barbeiroInativo);
            await _context.SaveChangesAsync();

            // 2. ACT
            var resultado = await _service.GetActiveBarbersAsync();

            // 3. ASSERT
            Assert.NotNull(resultado);
            Assert.Single(resultado); 
            Assert.Equal("Barbeiro Ativo", resultado.First().FullName);
        }

        // --- TESTE 2 ---
        [Fact]
        public async Task GetBarberByIdAsync_DeveRetornarBarbeiroCorreto_QuandoEncontrado()
        {
            // 1. Arrange
            await SeedDatabaseAsync(); // Cria o barbeiro com Id 1

            // 2. Act
            var resultado = await _service.GetBarberByIdAsync(1);

            // 3. Assert
            Assert.NotNull(resultado);
            Assert.Equal(1, resultado.BarberId);
            Assert.Equal("Barbeiro Teste", resultado.FullName);
            Assert.False(resultado.HasAcceptedTerms); 
        }

        // --- TESTE 3 ---
        [Fact]
        public async Task GetBarberByIdAsync_DeveRetornarNull_QuandoNaoEncontrado()
        {
            // 1. Arrange
            // Banco está vazio

            // 2. Act
            var resultado = await _service.GetBarberByIdAsync(99); // ID 99 não existe

            // 3. Assert
            Assert.Null(resultado);
        }

        // --- TESTE 4 ---
        [Fact]
        public async Task AcceptTermsAsync_DeveMudarHasAcceptedTermsParaTrue()
        {
            // 1. Arrange
            await SeedDatabaseAsync(); // Cria o barbeiro com Id 1 e HasAcceptedTerms = false

            // 2. Act
            var resultado = await _service.AcceptTermsAsync(1);

            // 3. Assert
            Assert.True(resultado); 
            
            var barbeiroDoBanco = await _context.Barbers.FindAsync(1);
            Assert.True(barbeiroDoBanco.HasAcceptedTerms);
        }

        // --- TESTE 5 ---
        [Fact]
        public async Task DeactivateBarberAsync_DeveMudarIsActiveParaFalse()
        {
            // 1. Arrange
            await SeedDatabaseAsync(); // Cria o barbeiro com Id 1 e IsActive = true
            
            _mockUserManager.Setup(um => um.FindByIdAsync("user1")).ReturnsAsync(_context.Users.Find("user1"));
            _mockUserManager.Setup(um => um.RemoveFromRoleAsync(It.IsAny<ApplicationUser>(), "Barbeiro")).ReturnsAsync(IdentityResult.Success);
            _mockUserManager.Setup(um => um.UpdateAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);

            // 2. Act
            var resultado = await _service.DeactivateBarberAsync(1);

            // 3. Assert
            Assert.True(resultado);
            
            var barbeiroDoBanco = await _context.Barbers.FindAsync(1);
            Assert.False(barbeiroDoBanco.IsActive); 
        }

        // --- TESTE 6 ---
        [Fact]
        public async Task UpdateBarberAsync_DeveAtualizarDadosCorretamente()
        {
            // 1. Arrange
            await SeedDatabaseAsync(); 
            
            var dto = new UpdateBarberDto 
            { 
                FullName = "Nome Novo", 
                Email = "novo@teste.com", 
                PhoneNumber = "12345", 
                Bio = "Bio Nova" 
            };
            
            _mockUserManager.Setup(um => um.FindByEmailAsync("novo@teste.com")).ReturnsAsync((ApplicationUser)null); 
            _mockUserManager.Setup(um => um.SetEmailAsync(It.IsAny<ApplicationUser>(), "novo@teste.com")).ReturnsAsync(IdentityResult.Success);
            _mockUserManager.Setup(um => um.SetUserNameAsync(It.IsAny<ApplicationUser>(), "novo@teste.com")).ReturnsAsync(IdentityResult.Success);
            _mockUserManager.Setup(um => um.UpdateAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);

            // 2. Act
            var result = await _service.UpdateBarberAsync(1, dto);

            // 3. Assert
            Assert.True(result); 
            
            var barbeiroDoBanco = await _context.Barbers.Include(b => b.UserAccount).FirstAsync(b => b.Id == 1);
            Assert.Equal("Nome Novo", barbeiroDoBanco.UserAccount.FullName);
            Assert.Equal("Bio Nova", barbeiroDoBanco.Bio);
            Assert.Equal("novo@teste.com", barbeiroDoBanco.UserAccount.Email);
        }

        // --- TESTE 7 ---
        [Fact]
        public async Task CreateBarberAsync_DeveCriarBarbeiroERetornarDto()
        {
            // 1. Arrange
            var dto = new CreateBarberDto
            {
                FullName = "Barbeiro Novo",
                Email = "novo@teste.com",
                Password = "Senha123!",
                PhoneNumber = "9999",
                Bio = "Eu sou novo"
            };

            _mockUserManager.Setup(um => um.FindByEmailAsync("novo@teste.com")).ReturnsAsync((ApplicationUser)null); 
            
            _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), "Senha123!"))
                .Callback<ApplicationUser, string>((user, pass) => 
                {
                    user.Id = "fake-user-id-123"; 
                })
                .ReturnsAsync(IdentityResult.Success);
            
            _mockUserManager.Setup(um => um.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Barbeiro")).ReturnsAsync(IdentityResult.Success);

            // 2. Act
            var resultado = await _service.CreateBarberAsync(dto);

            // 3. Assert
            Assert.NotNull(resultado);
            Assert.Equal("Barbeiro Novo", resultado.FullName);
            Assert.Equal("novo@teste.com", resultado.Email);
            Assert.False(resultado.HasAcceptedTerms); 
            
            Assert.Equal(1, await _context.Barbers.CountAsync()); 
            Assert.Equal("fake-user-id-123", (await _context.Barbers.FirstAsync()).ApplicationUserId);
        }

        // --- TESTE 8 ---
        [Fact]
        public async Task CreateBarberAsync_DeveLancarExcecao_QuandoEmailJaExiste()
        {
            // 1. Arrange
            var dto = new CreateBarberDto { Email = "email_existente@teste.com" };

            _mockUserManager.Setup(um => um.FindByEmailAsync("email_existente@teste.com"))
                            .ReturnsAsync(new ApplicationUser()); 

            // 2. Act & 3. Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateBarberAsync(dto));
            
            Assert.Equal("Este e-mail já está em uso.", exception.Message);
        }

        // --- ▼▼▼ TESTES NOVOS ADICIONADOS ▼▼▼ ---

        // --- TESTE 9 ---
        [Fact]
        public async Task UpdateBarberAsync_DeveLancarExcecao_QuandoEmailNovoJaEstaEmUso()
        {
            // 1. Arrange
            await SeedDatabaseAsync(); // Cria user1 (teste@teste.com)
            
            // Cria um segundo usuário/barbeiro que já possui o e-mail desejado
            var user2 = new ApplicationUser { Id = "user2", FullName = "Usuario Conflitante", Email = "conflito@teste.com" };
            _context.Users.Add(user2);
            await _context.SaveChangesAsync();

            var dto = new UpdateBarberDto 
            { 
                FullName = "Nome Novo", 
                Email = "conflito@teste.com" // Tenta mudar para o e-mail do user2
            };

            // Mock do FindByEmailAsync para retornar o user2 (conflito)
            _mockUserManager.Setup(um => um.FindByEmailAsync("conflito@teste.com")).ReturnsAsync(user2);

            // 2. Act & 3. Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateBarberAsync(1, dto));
            
            Assert.Equal("O novo e-mail fornecido já está em uso.", exception.Message);
        }

        // --- TESTE 10 ---
        [Fact]
        public async Task DeactivateBarberAsync_DeveRetornarFalse_QuandoBarbeiroNaoEncontrado()
        {
            // 1. Arrange
            // Banco está vazio

            // 2. Act
            var resultado = await _service.DeactivateBarberAsync(99); // ID 99 não existe

            // 3. Assert
            Assert.False(resultado);
        }
    }
}