using Xunit;
using Moq;
using Microsoft.AspNetCore.Identity;
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
    public class UserServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly AppDbContext _context;
        private readonly IUserService _service;

        // --- SETUP (Construtor) ---
        public UserServiceTests()
        {
            // 1. Configurar o Banco de Dados em Memória
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            
            _context = new AppDbContext(options);

            // 2. Configurar o Mock do UserManager
            var store = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);

            // 3. Instanciar o Serviço
            _service = new UserService(_mockUserManager.Object, _context);
        }

        // --- TESTE 1: RegisterCustomerAsync (Caminho Feliz) ---
        [Fact]
        public async Task RegisterCustomerAsync_DeveCriarClienteEEndereco_QuandoDadosSaoValidos()
        {
            // 1. Arrange
            var dto = new RegisterCustomerDto
            {
                FullName = "Cliente Teste",
                Email = "cliente@teste.com",
                Password = "Senha123!",
                Cep = "12345-678",
                Street = "Rua Teste",
                Number = "123",
                Neighborhood = "Bairro Teste",
                City = "Mogi",
                State = "SP"
            };

            // Mock do UserManager (usuário não existe, criação dá certo)
            _mockUserManager.Setup(um => um.FindByEmailAsync(dto.Email)).ReturnsAsync((ApplicationUser)null); // E-mail livre
            
            _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), dto.Password))
                .Callback<ApplicationUser, string>((user, pass) => 
                {
                    user.Id = "novo-cliente-id"; // Simula o Identity gerando um ID
                })
                .ReturnsAsync(IdentityResult.Success);
            
            _mockUserManager.Setup(um => um.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Cliente")).ReturnsAsync(IdentityResult.Success);

            // 2. Act
            var resultado = await _service.RegisterCustomerAsync(dto);

            // 3. Assert
            Assert.NotNull(resultado);
            Assert.Equal("Cliente Teste", resultado.FullName);
            Assert.Equal("novo-cliente-id", resultado.Id);

            // Verifica se o Endereço foi salvo no banco
            Assert.Equal(1, await _context.Addresses.CountAsync());
            var enderecoDoBanco = await _context.Addresses.FirstAsync();
            Assert.Equal("novo-cliente-id", enderecoDoBanco.ApplicationUserId);
            Assert.Equal("Rua Teste", enderecoDoBanco.Street);
        }

        // --- TESTE 2: RegisterCustomerAsync (E-mail Duplicado) ---
        [Fact]
        public async Task RegisterCustomerAsync_DeveLancarExcecao_QuandoEmailJaExiste()
        {
            // 1. Arrange
            var dto = new RegisterCustomerDto { Email = "email_existente@teste.com" };

            // Mock do UserManager (e-mail já existe!)
            _mockUserManager.Setup(um => um.FindByEmailAsync(dto.Email))
                            .ReturnsAsync(new ApplicationUser()); // Retorna um usuário (significa que já existe)

            // 2. Act & 3. Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.RegisterCustomerAsync(dto)
            );
            
            Assert.Equal("Este e-mail já está em uso.", exception.Message);
        }

        // --- TESTE 3: RegisterCustomerAsync (Falha no Identity) ---
        [Fact]
        public async Task RegisterCustomerAsync_DeveLancarExcecao_QuandoIdentityFalha()
        {
            // 1. Arrange
            var dto = new RegisterCustomerDto { Email = "cliente@teste.com", Password = "123" }; // Senha fraca

            // Mock do UserManager (e-mail livre, mas criação falha)
            _mockUserManager.Setup(um => um.FindByEmailAsync(dto.Email)).ReturnsAsync((ApplicationUser)null); 
            
            // Simula o Identity falhando (ex: senha muito curta)
            var identityErrors = new List<IdentityError> { new IdentityError { Description = "Senha muito curta" } };
            _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), dto.Password))
                            .ReturnsAsync(IdentityResult.Failed(identityErrors.ToArray()));

            // 2. Act & 3. Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.RegisterCustomerAsync(dto)
            );
            
            Assert.Contains("Falha ao criar usuário: Senha muito curta", exception.Message);
        }

        // --- TESTE 4: GetUserProfileAsync (Caminho Feliz) ---
        [Fact]
        public async Task GetUserProfileAsync_DeveRetornarPerfilCompleto_QuandoEncontrado()
        {
            // 1. Arrange
            var userId = "user1";
            var user = new ApplicationUser 
            { 
                Id = userId, 
                FullName = "Usuario Teste", 
                Email = "user@teste.com", 
                PhoneNumber = "1199999" 
            };
            
            // ▼▼▼ CORREÇÃO AQUI ▼▼▼
            // Adicionando os campos obrigatórios que faltavam
            var address = new Address
            {
                Id = 1,
                ApplicationUserId = userId,
                User = user,
                Street = "Rua X",
                City = "Mogi",
                Cep = "12345-000",
                Neighborhood = "Bairro Teste",
                Number = "100",
                State = "SP"
            };
            
            _context.Users.Add(user);
            _context.Addresses.Add(address);
            await _context.SaveChangesAsync(); // <-- Esta é a linha 152
            
            // Mock do UserManager para retornar o perfil
            var roles = new List<string> { "Cliente" };
            _mockUserManager.Setup(um => um.GetRolesAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(roles);

            // 2. Act
            var profile = await _service.GetUserProfileAsync(userId);

            // 3. Assert
            Assert.NotNull(profile);
            Assert.Equal("Usuario Teste", profile.FullName);
            Assert.Equal("user@teste.com", profile.Email);
            Assert.Contains("Cliente", profile.Roles);
            Assert.NotNull(profile.Address); // Verifica se o endereço veio
            Assert.Equal("Rua X", profile.Address.Street);
            Assert.Equal("12345-000", profile.Address.Cep); // Verifica o novo campo
        }

        // --- TESTE 5: GetUserProfileAsync (Usuário não encontrado) ---
        [Fact]
        public async Task GetUserProfileAsync_DeveLancarExcecao_QuandoNaoEncontrado()
        {
            // 1. Arrange
            // Banco vazio
            
            // 2. Act & 3. Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.GetUserProfileAsync("id-fantasma")
            );
        }
    }
}