using barbearia.api.Data;
using barbearia.api.Dtos;
using barbearia.api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace barbearia.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _context;

        public AuthController(UserManager<ApplicationUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpPost("register-customer")]
        public async Task<IActionResult> RegisterCustomer([FromBody] RegisterCustomerDto dto)
        {
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

            
            await _userManager.AddToRoleAsync(newUser, "Cliente");

            
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
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Cliente cadastrado com sucesso!" });
        }
    }
}
