using barbearia.api.Data;
using barbearia.api.Dtos;
using barbearia.api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace barbearia.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BarberController : ControllerBase
    {
        private readonly IBarberService _barberService;
        private readonly AppDbContext _context;

        // Construtor que injeta o serviço de barbeiros e o contexto do banco de dados
        public BarberController(IBarberService barberService, AppDbContext context)
        {
            _barberService = barberService;
            _context = context;
        }

        // Endpoint para listar todos os barbeiros ativos
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<BarberProfileDto>>> GetBarbers()
        {
            var barbers = await _barberService.GetActiveBarbersAsync();
            return Ok(barbers); // Retorna 200 com a lista de barbeiros ativos
        }

        // Endpoint para obter os dados de um barbeiro específico pelo ID
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Barbeiro")]
        public async Task<IActionResult> GetBarberById(int id)
        {
            try
            {
                var barberDto = await _barberService.GetBarberByIdAsync(id);

                if (barberDto == null)
                {
                    return NotFound("Barbeiro não encontrado."); // Retorna 404 se o barbeiro não for encontrado
                }

                return Ok(barberDto); // Retorna 200 com os dados do barbeiro
            }
            catch (Exception ex)
            {
                // Loga o erro e retorna 500 para erros internos
                return StatusCode(500, "Erro interno ao buscar dados do barbeiro.");
            }
        }

        // Endpoint para criar um novo perfil de barbeiro
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
                return BadRequest(ex.Message); // Retorna 400 se o e-mail já estiver em uso
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message); // Retorna 400 para erros do Identity
            }
            catch (Exception ex)
            {
                // Loga o erro e retorna 500 para erros internos
                return StatusCode(500, "Erro interno ao criar barbeiro.");
            }
        }

        // Endpoint para aceitar os termos de uso (barbeiro ou admin)
        [HttpPost("accept-terms")]
        [Authorize(Roles = "Barbeiro,Admin")]
        public async Task<IActionResult> AcceptTerms()
        {
            // Obtém o ID do usuário logado a partir do token
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized(); // Retorna 401 se o usuário não estiver autenticado
            }

            // Busca o perfil de barbeiro associado ao usuário logado
            var barber = await _context.Barbers.FirstOrDefaultAsync(b => b.ApplicationUserId == userId);

            if (barber == null)
            {
                return NotFound("Perfil de barbeiro não encontrado."); // Retorna 404 se o perfil não for encontrado
            }

            // Marca que o barbeiro aceitou os termos
            var result = await _barberService.AcceptTermsAsync(barber.Id);
            if (result)
            {
                return Ok(new { message = "Termos aceitos com sucesso." }); // Retorna 200 para sucesso
            }
            return BadRequest("Não foi possível salvar o aceite dos termos."); // Retorna 400 para falha
        }

        // Endpoint para atualizar os dados de um barbeiro
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Barbeiro")]
        public async Task<IActionResult> UpdateBarber(int id, [FromBody] UpdateBarberDto dto)
        {
            try
            {
                var success = await _barberService.UpdateBarberAsync(id, dto);
                if (!success)
                {
                    return NotFound("Barbeiro não encontrado."); // Retorna 404 se o barbeiro não for encontrado
                }
                return NoContent(); // Retorna 204 para sucesso
            }
            catch (ArgumentException ex) // Captura erro de e-mail duplicado
            {
                return BadRequest(ex.Message); // Retorna 400 com a mensagem de erro
            }
            catch (Exception ex)
            {
                // Loga o erro e retorna 500 para erros internos
                return StatusCode(500, "Erro interno ao atualizar barbeiro.");
            }
        }

        // Endpoint para desativar um barbeiro
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Barbeiro")]
        public async Task<IActionResult> DeactivateBarber(int id)
        {
            var success = await _barberService.DeactivateBarberAsync(id);
            if (!success)
            {
                return NotFound("Barbeiro não encontrado ou já inativo."); // Retorna 404 se o barbeiro não for encontrado ou já estiver inativo
            }
            return NoContent(); // Retorna 204 para sucesso
        }
    }
}