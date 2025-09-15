using barbearia.api.Data;
using barbearia.api.Dtos;
using barbearia.api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace barbearia.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BarberController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _context;

        public BarberController(UserManager<ApplicationUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<BarberProfileDto>>> GetBarbers()
        {

            var barbers = await _context.Barbers
                .Include(barbers => barbers.UserAccount)
                .Where(barbers => barbers.IsActive == true)
                .Select(barbers => new BarberProfileDto
                {
                    BarberId = barbers.Id,
                    UserId = barbers.ApplicationUserId,
                    FullName = barbers.UserAccount.FullName,
                    Email = barbers.UserAccount.Email,
                    PhoneNumber = barbers.UserAccount.PhoneNumber,
                    Bio = barbers.Bio
                })
                .ToListAsync();

            return Ok(barbers);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Barbeiro")]
        public async Task<ActionResult> CreateBarberProfile([FromBody] CreateBarberDto dto)
        {
            // 1. Verificar se o usuário já existe pelo email
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                return BadRequest("Este e-mail já está em uso.");
            }

            
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
                return BadRequest(identityResult.Errors);
            }

            
            await _userManager.AddToRoleAsync(newUser, "Barbeiro");

            
            var barberProfile = new Barber
            {
                ApplicationUserId = newUser.Id,
                Bio = dto.Bio ?? string.Empty
            };

            _context.Barbers.Add(barberProfile);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Barbeiro criado com sucesso." });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateBarber(int id, [FromBody] UpdateBarberDto dto)
        {
            var barberProfile = await _context.Barbers
                                    .Include(b => b.UserAccount)
                                    .FirstOrDefaultAsync(b => b.Id == id);

            if (barberProfile == null)
            {
                return NotFound("Perfil de barbeiro não encontrado.");
            }

            barberProfile.Bio = dto.Bio ?? barberProfile.Bio;

            var user = barberProfile.UserAccount;
            user.FullName = dto.FullName;
            user.PhoneNumber = dto.PhoneNumber;

            if (user.Email != dto.Email)
            {
                // Verifica se o NOVO email já não está em uso por outra pessoa
                var existingEmail = await _userManager.FindByEmailAsync(dto.Email);
                if (existingEmail != null && existingEmail.Id != user.Id)
                {
                    return BadRequest("O novo e-mail fornecido já está em uso por outra conta.");
                }

                // Define o novo email e força a confirmação (já que é um Admin que está mudando)
                var setEmailResult = await _userManager.SetEmailAsync(user, dto.Email);
                if (!setEmailResult.Succeeded) return BadRequest("Falha ao atualizar o e-mail.");

                user.EmailConfirmed = true;

                // Atualiza também o UserName (pois estamos usando o email como login)
                var setUserNameResult = await _userManager.SetUserNameAsync(user, dto.Email);
                if (!setUserNameResult.Succeeded) return BadRequest("Falha ao atualizar o nome de usuário.");
            }
 
            await _context.SaveChangesAsync();
            await _userManager.UpdateAsync(user);

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeactivateBarber(int id)
        {
            var barberProfile = await _context.Barbers.FindAsync(id);
            if (barberProfile == null)
            {
                return NotFound("Perfil de barbeiro não encontrado.");
            }

            
            barberProfile.IsActive = false;

            var user = await _userManager.FindByIdAsync(barberProfile.ApplicationUserId);
            if (user != null)
            {
                await _userManager.RemoveFromRoleAsync(user, "Barbeiro");
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
