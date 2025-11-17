using barbearia.api.Data;
using barbearia.api.Dtos;
using barbearia.api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace barbearia.api.Services
{
    public class BarberService : IBarberService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _context;

        // Construtor que recebe o UserManager e o DbContext
        public BarberService(UserManager<ApplicationUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // Retorna uma lista de barbeiros ativos
        public async Task<IEnumerable<BarberProfileDto>> GetActiveBarbersAsync()
        {
            return await _context.Barbers
                .Include(b => b.UserAccount) // Inclui os dados do usuário associado
                .Where(b => b.IsActive == true) // Filtra apenas barbeiros ativos
                .Select(b => new BarberProfileDto
                {
                    BarberId = b.Id,
                    UserId = b.ApplicationUserId,
                    FullName = b.UserAccount.FullName,
                    Email = b.UserAccount.Email,
                    PhoneNumber = b.UserAccount.PhoneNumber,
                    Bio = b.Bio
                })
                .ToListAsync();
        }

        // Retorna os dados de um barbeiro pelo ID
        public async Task<BarberProfileDto> GetBarberByIdAsync(int barberId)
        {
            var barber = await _context.Barbers
                .Include(b => b.UserAccount) // Inclui os dados do usuário associado
                .AsNoTracking() // Evita rastreamento para melhorar a performance
                .FirstOrDefaultAsync(b => b.Id == barberId);

            if (barber == null)
            {
                return null; // Retorna nulo se o barbeiro não for encontrado
            }

            // Mapeia os dados do barbeiro para o DTO
            return new BarberProfileDto
            {
                BarberId = barber.Id,
                UserId = barber.ApplicationUserId,
                FullName = barber.UserAccount.FullName,
                Email = barber.UserAccount.Email,
                PhoneNumber = barber.UserAccount.PhoneNumber,
                Bio = barber.Bio,
                HasAcceptedTerms = barber.HasAcceptedTerms
            };
        }

        // Cria um novo barbeiro
        public async Task<BarberProfileDto> CreateBarberAsync(CreateBarberDto dto)
        {
            // Verifica se o e-mail já está em uso
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                throw new ArgumentException("Este e-mail já está em uso.");
            }

            // Cria a conta de usuário (Identity)
            var newUser = new ApplicationUser
            {
                FullName = dto.FullName,
                Email = dto.Email,
                UserName = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                EmailConfirmed = true
            };
            var identityResult = await _userManager.CreateAsync(newUser, dto.Password);
            if (!identityResult.Succeeded)
            {
                throw new InvalidOperationException($"Falha ao criar conta de usuário: {string.Join(", ", identityResult.Errors.Select(e => e.Description))}");
            }

            // Adiciona o usuário ao papel "Barbeiro"
            await _userManager.AddToRoleAsync(newUser, "Barbeiro");

            // Cria o perfil do barbeiro
            var barberProfile = new Barber
            {
                ApplicationUserId = newUser.Id,
                Bio = dto.Bio ?? string.Empty,
                IsActive = true, // Barbeiro começa ativo por padrão
                HasAcceptedTerms = false
            };
            _context.Barbers.Add(barberProfile);
            await _context.SaveChangesAsync();

            // Retorna os dados do barbeiro criado
            return new BarberProfileDto
            {
                BarberId = barberProfile.Id,
                UserId = newUser.Id,
                FullName = newUser.FullName,
                Email = newUser.Email,
                PhoneNumber = newUser.PhoneNumber,
                Bio = barberProfile.Bio
            };
        }

        // Atualiza os dados de um barbeiro
        public async Task<bool> UpdateBarberAsync(int barberId, UpdateBarberDto dto)
        {
            var barberProfile = await _context.Barbers
                                        .Include(b => b.UserAccount) // Inclui os dados do usuário associado
                                        .FirstOrDefaultAsync(b => b.Id == barberId);
            if (barberProfile == null) return false;

            // Atualiza o perfil do barbeiro
            barberProfile.Bio = dto.Bio ?? barberProfile.Bio;

            // Atualiza os dados do usuário associado
            var user = barberProfile.UserAccount;
            user.FullName = dto.FullName;
            user.PhoneNumber = dto.PhoneNumber;

            // Atualiza o e-mail se necessário
            if (user.Email != dto.Email)
            {
                var existingEmail = await _userManager.FindByEmailAsync(dto.Email);
                if (existingEmail != null && existingEmail.Id != user.Id)
                {
                    throw new ArgumentException("O novo e-mail fornecido já está em uso.");
                }
                await _userManager.SetEmailAsync(user, dto.Email);
                await _userManager.SetUserNameAsync(user, dto.Email);

                // Reconfirma o e-mail
                user.Email = dto.Email;
                user.UserName = dto.Email;
                user.EmailConfirmed = true;
            }

            // Salva as alterações no banco de dados
            await _context.SaveChangesAsync();
            var userUpdateResult = await _userManager.UpdateAsync(user);

            return userUpdateResult.Succeeded;
        }

        // Desativa um barbeiro
        public async Task<bool> DeactivateBarberAsync(int barberId)
        {
            var barberProfile = await _context.Barbers.FindAsync(barberId);
            if (barberProfile == null || !barberProfile.IsActive) return false; // Já inativo ou não encontrado

            // Marca o barbeiro como inativo
            barberProfile.IsActive = false;

            // Remove o papel "Barbeiro" e bloqueia a conta
            var user = await _userManager.FindByIdAsync(barberProfile.ApplicationUserId);
            if (user != null)
            {
                await _userManager.RemoveFromRoleAsync(user, "Barbeiro");
                user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100); // Bloqueia a conta
                await _userManager.UpdateAsync(user);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // Marca que o barbeiro aceitou os termos
        public async Task<bool> AcceptTermsAsync(int barberId)
        {
            var barberProfile = await _context.Barbers.FindAsync(barberId);
            if (barberProfile == null)
            {
                return false;
            }

            barberProfile.HasAcceptedTerms = true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
