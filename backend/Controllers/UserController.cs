using barbearia.api.Dtos;
using barbearia.api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace barbearia.api.Controllers
{
    [ApiController]
    [Route("api/users")] // Rota base: https://seu-site.com/api/users
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // ==========================================
        //  MÉTODOS PARA O ADMIN (IGUAL BARBEIRO)
        // ==========================================

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
                var result = await _userService.DeleteUserAccountAsync(userId);
                if (result != null && !result.Succeeded) return BadRequest(result.Errors);
                return NoContent();
            }
            catch (Exception) { return StatusCode(500, "Erro ao deletar conta."); }
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