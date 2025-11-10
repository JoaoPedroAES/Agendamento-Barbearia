using barbearia.api.Data; // <-- 1. ADICIONE ESTE 'USING'
using barbearia.api.Dtos;
using barbearia.api.Models;
using barbearia.api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Mail;
using System.Security.Claims;

namespace barbearia.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BarberController : ControllerBase
    {
        private readonly IBarberService _barberService;
        private readonly AppDbContext _context; // <-- 2. ADICIONE ESTE CAMPO

        // 3. ATUALIZE O CONSTRUTOR PARA RECEBER O AppDbContext
        public BarberController(IBarberService barberService, AppDbContext context)
        {
            _barberService = barberService;
            _context = context; // <-- 4. ATRIBUA O CAMPO
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<BarberProfileDto>>> GetBarbers()
        {
            var barbers = await _barberService.GetActiveBarbersAsync();
            return Ok(barbers);
        }
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Barbeiro")]
        public async Task<IActionResult> GetBarberById(int id)
        {
            try
            {
                var barberDto = await _barberService.GetBarberByIdAsync(id);

                if (barberDto == null)
                {
                    return NotFound("Barbeiro não encontrado.");
                }

                return Ok(barberDto);
            }
            catch (Exception ex)
            {
                // Logar o erro (ex.Message)
                return StatusCode(500, "Erro interno ao buscar dados do barbeiro.");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Barbeiro")]
        public async Task<ActionResult<BarberProfileDto>> CreateBarberProfile([FromBody] CreateBarberDto dto)
        {
            try
            {
                var createdBarber = await _barberService.CreateBarberAsync(dto);
                // Retorna 201 Created com o perfil criado
                return CreatedAtAction(nameof(GetBarbers), new { id = createdBarber.BarberId }, createdBarber);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message); // E-mail duplicado
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message); // Erro do Identity
            }
            catch (Exception ex)
            {
                // Logar erro ex
                return StatusCode(500, "Erro interno ao criar barbeiro.");
            }
        }

        [HttpPost("accept-terms")]
        [Authorize(Roles = "Barbeiro,Admin")]
        public async Task<IActionResult> AcceptTerms()
        {
            // Pega o ID do usuário logado (pelo token)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            // Encontra o perfil de barbeiro associado a esse usuário
            // AGORA ESTA LINHA VAI FUNCIONAR:
            var barber = await _context.Barbers.FirstOrDefaultAsync(b => b.ApplicationUserId == userId);

            if (barber == null)
            {
                return NotFound("Perfil de barbeiro não encontrado.");
            }

            var result = await _barberService.AcceptTermsAsync(barber.Id);
            if (result)
            {
                return Ok(new { message = "Termos aceitos com sucesso." });
            }
            return BadRequest("Não foi possível salvar o aceite dos termos.");
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Barbeiro")]
        public async Task<IActionResult> UpdateBarber(int id, [FromBody] UpdateBarberDto dto)
        {
            try
            {
                var success = await _barberService.UpdateBarberAsync(id, dto);
                if (!success)
                {
                    return NotFound("Barbeiro não encontrado.");
                }
                return NoContent(); // 204 Sucesso
            }
            catch (ArgumentException ex) // E-mail duplicado
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                // Logar erro ex
                return StatusCode(500, "Erro interno ao atualizar barbeiro.");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Barbeiro")]
        public async Task<IActionResult> DeactivateBarber(int id)
        {
            var success = await _barberService.DeactivateBarberAsync(id);
            if (!success)
            {
                return NotFound("Barbeiro não encontrado ou já inativo.");
            }
            return NoContent(); // 204 Sucesso
        }
    }
}