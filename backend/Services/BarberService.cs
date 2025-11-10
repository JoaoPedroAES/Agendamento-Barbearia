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

        public BarberService(UserManager<ApplicationUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IEnumerable<BarberProfileDto>> GetActiveBarbersAsync()
        {
            return await _context.Barbers
                .Include(b => b.UserAccount)
                .Where(b => b.IsActive == true) // Filtro já aplicado aqui
                .Select(b => new BarberProfileDto
                {
                    BarberId = b.Id,
                    UserId = b.ApplicationUserId,
                    FullName = b.UserAccount.FullName,
                    Email = b.UserAccount.Email,
                    PhoneNumber = b.UserAccount.PhoneNumber, // Incluído
                    Bio = b.Bio
                })
                .ToListAsync();
        }

        public async Task<BarberProfileDto> GetBarberByIdAsync(int barberId)
        {
            var barber = await _context.Barbers
                .Include(b => b.UserAccount)
                .AsNoTracking() // <--- ESTA É A CORREÇÃO
                .FirstOrDefaultAsync(b => b.Id == barberId);

            if (barber == null)
            {
                return null; // Retorna nulo se não encontrar
            }

            // Mapeia para o DTO
            return new BarberProfileDto
            {
                BarberId = barber.Id,
                UserId = barber.ApplicationUserId,
                FullName = barber.UserAccount.FullName,
                Email = barber.UserAccount.Email,
                PhoneNumber = barber.UserAccount.PhoneNumber,
                Bio = barber.Bio,
                HasAcceptedTerms = barber.HasAcceptedTerms // <-- Importante!
            };
        }

        public async Task<BarberProfileDto> CreateBarberAsync(CreateBarberDto dto)
        {
            // 1. Verificar se o e-mail já existe
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                throw new ArgumentException("Este e-mail já está em uso.");
            }

            // 2. Criar a conta de usuário (Identity)
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

            // 3. Adicionar ao Perfil "Barbeiro"
            await _userManager.AddToRoleAsync(newUser, "Barbeiro");

            // 4. Criar o Perfil de Barbeiro
            var barberProfile = new Barber
            {
                ApplicationUserId = newUser.Id,
                Bio = dto.Bio ?? string.Empty,
                IsActive = true, // Novo barbeiro começa ativo
                HasAcceptedTerms = false
            };
            _context.Barbers.Add(barberProfile);
            await _context.SaveChangesAsync();

            // 5. Retornar o DTO do perfil criado
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

        public async Task<bool> UpdateBarberAsync(int barberId, UpdateBarberDto dto)
        {
            var barberProfile = await _context.Barbers
                                        .Include(b => b.UserAccount)
                                        .FirstOrDefaultAsync(b => b.Id == barberId);
            if (barberProfile == null) return false;

            // Atualiza Perfil (Barber)
            barberProfile.Bio = dto.Bio ?? barberProfile.Bio;

            // Atualiza Conta (ApplicationUser)
            var user = barberProfile.UserAccount;
            user.FullName = dto.FullName;
            user.PhoneNumber = dto.PhoneNumber;

            // Atualiza Email/UserName se necessário
            if (user.Email != dto.Email)
            {
                var existingEmail = await _userManager.FindByEmailAsync(dto.Email);
                if (existingEmail != null && existingEmail.Id != user.Id)
                {
                    throw new ArgumentException("O novo e-mail fornecido já está em uso.");
                }
                await _userManager.SetEmailAsync(user, dto.Email);
                await _userManager.SetUserNameAsync(user, dto.Email);

                // ▼▼▼ LINHAS FALTANTES ADICIONADAS AQUI ▼▼▼
                user.Email = dto.Email;
                user.UserName = dto.Email;
                user.EmailConfirmed = true; // Reconfirma
            }

            // Salva
            await _context.SaveChangesAsync(); // <-- Agora isso vai salvar o e-mail novo
            var userUpdateResult = await _userManager.UpdateAsync(user);

            return userUpdateResult.Succeeded;
        }

        public async Task<bool> DeactivateBarberAsync(int barberId)
        {
            var barberProfile = await _context.Barbers.FindAsync(barberId);
            if (barberProfile == null || !barberProfile.IsActive) return false; // Já inativo ou não encontrado

            // Marca como inativo
            barberProfile.IsActive = false;

            // Remove Role "Barbeiro" e tranca a conta (opcional, mas seguro)
            var user = await _userManager.FindByIdAsync(barberProfile.ApplicationUserId);
            if (user != null)
            {
                await _userManager.RemoveFromRoleAsync(user, "Barbeiro");
                user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100); // Tranca
                await _userManager.UpdateAsync(user);
            }

            await _context.SaveChangesAsync();
            return true;
        }

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
