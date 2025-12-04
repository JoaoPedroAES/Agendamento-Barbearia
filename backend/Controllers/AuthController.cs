using barbearia.api.Dtos;
using barbearia.api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

// IMPORTANTE: Adicione esta linha! (Verifique se o nome da pasta é Models ou Entities)
using barbearia.api.Models;

namespace barbearia.api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        // 1. MUDANÇA AQUI: De IdentityUser para ApplicationUser
        private readonly UserManager<ApplicationUser> _userManager;

        // 2. MUDANÇA AQUI TAMBÉM: No construtor
        public AuthController(IUserService userService, UserManager<ApplicationUser> userManager)
        {
            _userService = userService;
            _userManager = userManager;
        }

        [HttpPost("register-customer")]
        public async Task<IActionResult> RegisterCustomer([FromBody] RegisterCustomerDto dto)
        {
            // ... (o código aqui continua igual) ...
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var user = await _userService.RegisterCustomerAsync(dto);
                return StatusCode(201, new { Message = $"{(dto.CreateAsAdmin ? "Administrador" : "Cliente")} cadastrado com sucesso!" });
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro inesperado em RegisterCustomer: {ex}");
                return StatusCode(500, "Ocorreu um erro interno durante o cadastro.");
            }
        }

        [HttpPost("validate-reset-customer")]
        [AllowAnonymous]
        public async Task<IActionResult> ValidateResetCustomer([FromBody] ValidateResetRequestDto request)
        {
            // O _userManager agora sabe lidar com a tua classe personalizada
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                return NotFound("Dados não conferem."); // Usuário não existe
            }

            // Verifica telefone
            if (user.PhoneNumber != request.PhoneNumber)
            {
                return NotFound("Dados não conferem.");
            }

            // Verifica Role
            var isCliente = await _userManager.IsInRoleAsync(user, "Cliente");
            if (!isCliente)
            {
                return NotFound("Apenas clientes podem recuperar senha por aqui.");
            }

            return Ok(new { message = "Dados validados com sucesso." });
        }

        [HttpPost("reset-password-customer")]
        [AllowAnonymous] // Permite acesso sem login
        public async Task<IActionResult> ResetPasswordCustomer([FromBody] ResetPasswordCustomerDto request)
        {
            // 1. Busca o usuário
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                return NotFound("Usuário não encontrado.");
            }

            // 2. Segurança Extra: Confirma se é Cliente
            var isCliente = await _userManager.IsInRoleAsync(user, "Cliente");
            if (!isCliente)
            {
                return BadRequest("Permissão negada.");
            }

            // 3. O Pulo do Gato: Gerar o token de reset internamente
            // O Identity precisa desse token para autorizar a troca sem a senha antiga
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // 4. Efetuar a troca da senha
            var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);

            if (!result.Succeeded)
            {
                // Se a senha for muito fraca (ex: "123"), o Identity vai reclamar aqui
                return BadRequest(result.Errors);
            }

            return Ok(new { message = "Senha alterada com sucesso!" });
        }
    }
}