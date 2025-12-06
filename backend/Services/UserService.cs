using barbearia.api.Data;
using barbearia.api.Dtos;
using barbearia.api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace barbearia.api.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _context;

        // Construtor que recebe o UserManager e o DbContext
        public UserService(UserManager<ApplicationUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // Registra um novo cliente no sistema
        public async Task<ApplicationUser> RegisterCustomerAsync(RegisterCustomerDto dto)
        {
            // Verifica se o e-mail já está em uso
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                throw new ArgumentException("Este e-mail já está em uso.");
            }

            // Cria o usuário com os dados fornecidos
            var newUser = new ApplicationUser
            {
                FullName = dto.FullName,
                Email = dto.Email,
                UserName = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                EmailConfirmed = true // Marca o e-mail como confirmado
            };

            // Tenta criar o usuário no Identity
            var identityResult = await _userManager.CreateAsync(newUser, dto.Password);
            if (!identityResult.Succeeded)
            {
                // Lança exceção com os erros do Identity
                throw new InvalidOperationException($"Falha ao criar usuário: {string.Join(", ", identityResult.Errors.Select(e => e.Description))}");
            }

            // Adiciona o usuário ao papel (Admin ou Cliente)
            var role = dto.CreateAsAdmin ? "Admin" : "Cliente";
            await _userManager.AddToRoleAsync(newUser, role);

            // Cria o endereço do usuário
            var address = new Address
            {
                ApplicationUserId = newUser.Id,
                Cep = dto.Cep,
                Street = dto.Street,
                Number = dto.Number,
                Complement = dto.Complement,
                Neighborhood = dto.Neighborhood,
                City = dto.City,
                State = dto.State
            };

            // Salva o endereço no banco de dados
            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();

            return newUser; // Retorna o usuário criado
        }

        // Retorna o perfil completo de um usuário com base no ID
        public async Task<UserProfileDto> GetUserProfileAsync(string userId)
        {
            // Busca o usuário no banco de dados, incluindo o endereço
            var user = await _context.Users
                            .Include(u => u.Address)
                            .AsNoTracking() // Apenas leitura
                            .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) throw new KeyNotFoundException("Usuário não encontrado.");

            // Obtém os papéis do usuário
            var roles = await _userManager.GetRolesAsync(user);
            int? barberId = null;

            // Verifica se o usuário é um barbeiro e obtém o ID do perfil de barbeiro
            if (roles.Contains("Barbeiro"))
            {
                var barberProfile = await _context.Barbers.AsNoTracking().FirstOrDefaultAsync(b => b.ApplicationUserId == userId);
                if (barberProfile != null) barberId = barberProfile.Id;
            }

            // Retorna os dados do perfil do usuário
            return new UserProfileDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Roles = roles,
                BarberId = barberId,
                Address = user.Address == null ? null : new AddressDto
                {
                    Cep = user.Address.Cep,
                    Street = user.Address.Street,
                    Number = user.Address.Number,
                    Complement = user.Address.Complement,
                    Neighborhood = user.Address.Neighborhood,
                    City = user.Address.City,
                    State = user.Address.State
                }
            };
        }

        // Atualiza o perfil de um usuário com base no ID e nos dados fornecidos
        public async Task<bool> UpdateUserProfileAsync(string userId, UpdateProfileDto dto)
        {
            // Busca o usuário pelo ID
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false; // Retorna false se o usuário não for encontrado

            // Atualiza os dados do usuário
            user.FullName = dto.FullName;
            user.PhoneNumber = dto.PhoneNumber;

            // Atualiza ou cria o endereço do usuário
            var address = await _context.Addresses.FirstOrDefaultAsync(a => a.ApplicationUserId == userId);
            if (address == null)
            {
                address = new Address { ApplicationUserId = userId };
                _context.Addresses.Add(address);
            }
            address.Cep = dto.Cep;
            address.Street = dto.Street;
            address.Number = dto.Number;
            address.Complement = dto.Complement;
            address.Neighborhood = dto.Neighborhood;
            address.City = dto.City;
            address.State = dto.State;

            // Salva as alterações no banco de dados
            var result = await _userManager.UpdateAsync(user);
            await _context.SaveChangesAsync();

            return result.Succeeded; // Retorna true se a atualização foi bem-sucedida
        }

        // --- ATENÇÃO: MÉTODO ATUALIZADO PARA LIBERAR HORÁRIOS ---
        public async Task<IdentityResult?> DeleteUserAccountAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new KeyNotFoundException("Usuário não encontrado.");

            // --- CORREÇÃO: Simplificamos a busca ---
            // Buscamos QUALQUER agendamento que esteja como 'Scheduled' (Agendado)
            // Removemos a verificação de data para evitar problemas de Fuso Horário
            var activeAppointments = await _context.Appointments
                .Where(a => a.CustomerId == userId &&
                            a.Status == AppointmentStatus.Scheduled)
                .ToListAsync();

            // Cancela todos eles
            foreach (var appointment in activeAppointments)
            {
                appointment.Status = AppointmentStatus.CancelledByCustomer;
            }

            // Salva o cancelamento
            await _context.SaveChangesAsync();

            // --- Restante do código de anonimização (igual ao anterior) ---
            var address = await _context.Addresses.FirstOrDefaultAsync(a => a.ApplicationUserId == userId);
            if (address != null)
            {
                _context.Addresses.Remove(address);
                await _context.SaveChangesAsync();
            }

            user.FullName = "Usuário Removido";
            user.PhoneNumber = null;
            var anonymousEmail = $"deleted_{user.Id}@{Guid.NewGuid()}.com";
            user.Email = anonymousEmail;
            user.NormalizedEmail = anonymousEmail.ToUpperInvariant();
            user.UserName = anonymousEmail;
            user.NormalizedUserName = anonymousEmail.ToUpperInvariant();
            user.PasswordHash = null;
            user.LockoutEnd = DateTime.UtcNow.AddYears(100);
            user.EmailConfirmed = false;

            return await _userManager.UpdateAsync(user);
        }

        public async Task<List<UserProfileDto>> GetAllUsersAsync()
        {
            var users = await _userManager.GetUsersInRoleAsync("Cliente");

            return users.Select(u => new UserProfileDto
            {
                Id = u.Id, // <--- OBRIGATÓRIO: Preencher o ID aqui!
                FullName = u.FullName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber
            }).ToList();
        }
    }
}