using barbearia.api.Data;
using barbearia.api.Dtos;
using barbearia.api.Models;
using barbearia.api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace barbearia.api.Controllers
{
    [ApiController]
    [Route("api/users")] // Rota base: https://seu-site.com/api/users
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly AppDbContext _context;

        public UserController(IUserService userService, AppDbContext context)
        {
            _userService = userService;
            _context = context;
        }


        // 1. LISTAR TODOS OS CLIENTES
        // GET: api/users
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                // Lembre-se: Você precisa ter criado o GetAllUsersAsync no Service
                var users = await _userService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erro ao listar usuários.");
            }
        }

        // 2. EXCLUIR/ANONIMIZAR UM CLIENTE ESPECÍFICO
        // DELETE: api/users/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUserById(string id)
        {
            try
            {
                // Reutiliza a lógica de anonimização que você já tinha
                var result = await _userService.DeleteUserAccountAsync(id);

                if (result != null && !result.Succeeded)
                {
                    return BadRequest(result.Errors);
                }

                return NoContent(); // 204 = Sucesso, sem conteúdo (padrão de delete)
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Usuário não encontrado.");
            }
            catch (Exception)
            {
                return StatusCode(500, "Erro ao deletar conta.");
            }
        }

        // ==========================================
        //  MÉTODOS DO PRÓPRIO USUÁRIO (PERFIL)
        // ==========================================

        // 3. MEU PERFIL
        // GET: api/users/me
        [HttpGet("me")]
        public async Task<ActionResult<UserProfileDto>> GetMyProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            try
            {
                var userProfile = await _userService.GetUserProfileAsync(userId);
                return Ok(userProfile);
            }
            catch (Exception) { return StatusCode(500, "Erro ao buscar perfil."); }
        }

        // 4. EXCLUIR MINHA CONTA (AUTO-EXCLUSÃO)
        // DELETE: api/users/me
        [HttpDelete("me")]
        public async Task<IActionResult> DeleteMyAccount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            try
            {
                // Verifica se o cliente tem agendamentos pendentes
                bool hasPendingAppointments = await _context.Appointments
                    .AnyAsync(a => a.CustomerId == userId && a.Status == AppointmentStatus.Scheduled);

                if (hasPendingAppointments)
                {
                    // Retorna um erro 400 (Bad Request) com uma mensagem clara para o Frontend
                    return BadRequest(new { message = "Você não pode excluir sua conta pois possui agendamentos pendentes. Cancele-os primeiro." });
                }

                var result = await _userService.DeleteUserAccountAsync(userId);
                if (result != null && !result.Succeeded) return BadRequest(result.Errors);

                return NoContent(); // 204 No Content (Sucesso, sem corpo na resposta)
            }
            catch (Exception)
            {
                return StatusCode(500, "Erro ao deletar conta.");
            }
        }

        // 5. ATUALIZAR MEU PERFIL
        // PUT: api/users/me
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            // ... (restante do código igual ao que você já tinha)
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var success = await _userService.UpdateUserProfileAsync(userId, dto);
                if (!success) return NotFound();
                return NoContent();
            }
            catch { return StatusCode(500); }
        }
    }
}