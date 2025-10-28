using barbearia.api.Data;
using barbearia.api.Dtos;
using barbearia.api.Models;
using barbearia.api.Services;
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
        private readonly IUserService _userService;

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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            try
            {
                var result = await _userService.DeleteUserAccountAsync(userId);
                if (result != null && !result.Succeeded)
                {
                    // Retorna os erros do Identity se a atualização falhar
                    return BadRequest(result.Errors);
                }
                return NoContent(); // 204 Sucesso
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                // Logar erro ex
                return StatusCode(500, "Erro ao deletar conta.");
            }
        }

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
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                // Logar erro ex
                return StatusCode(500, "Erro ao buscar perfil.");
            }
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var success = await _userService.UpdateUserProfileAsync(userId, dto);
                if (!success) return NotFound("Usuário não encontrado.");
                return NoContent(); // 204 Sucesso
            }
            catch (Exception ex)
            {
                // Logar erro ex
                return StatusCode(500, "Erro ao atualizar perfil.");
            }
        }
    }
}
