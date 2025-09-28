using barbearia.api.Data;
using barbearia.api.Dtos;
using barbearia.api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace barbearia.api.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _context;

        public UserController(UserManager<ApplicationUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        /// <summary>
        /// Anonimiza os dados do usuário logado e desativa permanentemente sua conta (LGPD).
        /// </summary>
        [HttpDelete("me")]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> DeleteMyAccount()
        {
            
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("Usuário não encontrado.");
            }

            
            var address = await _context.Addresses.FirstOrDefaultAsync(a => a.ApplicationUserId == userId);
            if (address != null)
            {
                _context.Addresses.Remove(address);
                await _context.SaveChangesAsync(); // Salva a remoção do endereço
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

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                
                return BadRequest(result.Errors);
            }

            return NoContent();
        }

        [HttpGet("me")]
        public async Task<ActionResult<UserProfileDto>> GetMyProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            // Usamos .Include() para carregar o endereço junto com o usuário
            var user = await _context.Users
                            .Include(u => u.Address)
                            .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound("Usuário não encontrado.");

            var userProfileDto = new UserProfileDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                // Mapeia os dados do endereço para o DTO
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

            return Ok(userProfileDto);
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("Usuário não encontrado.");

            // Atualiza os dados da conta (Tabela AspNetUsers)
            user.FullName = dto.FullName;
            user.PhoneNumber = dto.PhoneNumber;

            // Atualiza os dados do endereço (Tabela Addresses)
            var address = await _context.Addresses.FirstOrDefaultAsync(a => a.ApplicationUserId == userId);
            if (address != null)
            {
                address.Cep = dto.Cep;
                address.Street = dto.Street;
                address.Number = dto.Number;
                address.Complement = dto.Complement;
                address.Neighborhood = dto.Neighborhood;
                address.City = dto.City;
                address.State = dto.State;
            }

            // Salva as alterações
            var result = await _userManager.UpdateAsync(user);
            await _context.SaveChangesAsync();

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return NoContent(); // Sucesso
        }
    }
}
