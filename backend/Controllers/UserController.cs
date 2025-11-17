using barbearia.api.Dtos;
using barbearia.api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace barbearia.api.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        // Construtor que injeta o serviço de usuários
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Anonimiza os dados do usuário logado e desativa permanentemente sua conta (LGPD).
        /// </summary>
        [HttpDelete("me")]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> DeleteMyAccount()
        {
            // Obtém o ID do usuário logado a partir do token
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized(); // Retorna 401 se o usuário não estiver autenticado

            try
            {
                // Chama o serviço para excluir a conta do usuário
                var result = await _userService.DeleteUserAccountAsync(userId);
                if (result != null && !result.Succeeded)
                {
                    // Retorna 400 com os erros do Identity se a exclusão falhar
                    return BadRequest(result.Errors);
                }
                return NoContent(); // Retorna 204 para sucesso
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message); // Retorna 404 se o usuário não for encontrado
            }
            catch (Exception ex)
            {
                // Loga o erro e retorna 500 para erros internos
                return StatusCode(500, "Erro ao deletar conta.");
            }
        }

        /// <summary>
        /// Retorna o perfil do usuário logado.
        /// </summary>
        [HttpGet("me")]
        public async Task<ActionResult<UserProfileDto>> GetMyProfile()
        {
            // Obtém o ID do usuário logado a partir do token
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized(); // Retorna 401 se o usuário não estiver autenticado

            try
            {
                // Chama o serviço para buscar o perfil do usuário
                var userProfile = await _userService.GetUserProfileAsync(userId);
                return Ok(userProfile); // Retorna 200 com os dados do perfil
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message); // Retorna 404 se o usuário não for encontrado
            }
            catch (Exception ex)
            {
                // Loga o erro e retorna 500 para erros internos
                return StatusCode(500, "Erro ao buscar perfil.");
            }
        }

        /// <summary>
        /// Atualiza o perfil do usuário logado.
        /// </summary>
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileDto dto)
        {
            // Obtém o ID do usuário logado a partir do token
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized(); // Retorna 401 se o usuário não estiver autenticado

            // Valida o modelo recebido
            if (!ModelState.IsValid) return BadRequest(ModelState); // Retorna 400 se o modelo for inválido

            try
            {
                // Chama o serviço para atualizar o perfil do usuário
                var success = await _userService.UpdateUserProfileAsync(userId, dto);
                if (!success) return NotFound("Usuário não encontrado."); // Retorna 404 se o usuário não for encontrado
                return NoContent(); // Retorna 204 para sucesso
            }
            catch (Exception ex)
            {
                // Loga o erro e retorna 500 para erros internos
                return StatusCode(500, "Erro ao atualizar perfil.");
            }
        }
    }
}
