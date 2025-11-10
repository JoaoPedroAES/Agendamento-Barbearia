using barbearia.api.Data;
using barbearia.api.Dtos;
using barbearia.api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;


namespace barbearia.api.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _context;

        public UserService(UserManager<ApplicationUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<ApplicationUser> RegisterCustomerAsync(RegisterCustomerDto dto)
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                throw new ArgumentException("Este e-mail já está em uso.");
            }

            // 1. Criar o Usuário
            var newUser = new ApplicationUser
            {
                FullName = dto.FullName,
                Email = dto.Email,
                UserName = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                EmailConfirmed = true // Simplificação: já marca como confirmado
            };

            var identityResult = await _userManager.CreateAsync(newUser, dto.Password);
            if (!identityResult.Succeeded)
            {
                // Lança uma exceção com os erros do Identity
                throw new InvalidOperationException($"Falha ao criar usuário: {string.Join(", ", identityResult.Errors.Select(e => e.Description))}");
            }

            // 2. Adicionar ao Perfil correto
            var role = dto.CreateAsAdmin ? "Admin" : "Cliente";
            await _userManager.AddToRoleAsync(newUser, role);

            // 3. Criar o Endereço
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

            _context.Addresses.Add(address);
            await _context.SaveChangesAsync(); // Salva apenas o endereço aqui

            return newUser; // Retorna o usuário criado
        }
        public async Task<UserProfileDto> GetUserProfileAsync(string userId)
        {
            var user = await _context.Users
                            .Include(u => u.Address) // Inclui o endereço
                            .AsNoTracking() // Leitura apenas
                            .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) throw new KeyNotFoundException("Usuário não encontrado.");

            var roles = await _userManager.GetRolesAsync(user);
            int? barberId = null;
            if (roles.Contains("Barbeiro"))
            {
                var barberProfile = await _context.Barbers.AsNoTracking().FirstOrDefaultAsync(b => b.ApplicationUserId == userId);
                if (barberProfile != null) barberId = barberProfile.Id;
            }

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
        public async Task<bool> UpdateUserProfileAsync(string userId, UpdateProfileDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false; // Usuário não encontrado

            // Atualiza dados do usuário
            user.FullName = dto.FullName;
            user.PhoneNumber = dto.PhoneNumber;

            // Atualiza dados do endereço (ou cria se não existir)
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

            // Salva
            var result = await _userManager.UpdateAsync(user);
            await _context.SaveChangesAsync(); // Salva endereço

            return result.Succeeded;
        }
        public async Task<IdentityResult?> DeleteUserAccountAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new KeyNotFoundException("Usuário não encontrado.");

            // Remove endereço
            var address = await _context.Addresses.FirstOrDefaultAsync(a => a.ApplicationUserId == userId);
            if (address != null)
            {
                _context.Addresses.Remove(address);
                await _context.SaveChangesAsync();
            }

            // Anonimiza dados
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

            // Atualiza o usuário no Identity
            var result = await _userManager.UpdateAsync(user);
            return result; // Retorna o IdentityResult para o controller saber se deu certo
        }
    }
}
