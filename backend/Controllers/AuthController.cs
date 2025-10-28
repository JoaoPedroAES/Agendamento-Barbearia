using barbearia.api.Dtos;         // Para RegisterCustomerDto
using barbearia.api.Services;     // Para IUserService
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace barbearia.api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        // ÚNICA DEPENDÊNCIA: O Serviço de Usuário
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register-customer")]
        public async Task<IActionResult> RegisterCustomer([FromBody] RegisterCustomerDto dto)
        {
            // Validação básica do modelo recebido
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Delega a lógica de registro para o serviço
                var user = await _userService.RegisterCustomerAsync(dto);

                // Retorna 201 Created (ou 200 OK) com mensagem de sucesso
                return StatusCode(201, new { Message = $"{(dto.CreateAsAdmin ? "Administrador" : "Cliente")} cadastrado com sucesso!" });
            }
            catch (ArgumentException ex) // Captura erro de e-mail duplicado vindo do serviço
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex) // Captura erros do Identity vindos do serviço
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex) // Captura erros genéricos
            {
                // É uma boa prática logar o erro real para depuração
                Console.WriteLine($"Erro inesperado em RegisterCustomer: {ex}"); // Log simples
                return StatusCode(500, "Ocorreu um erro interno durante o cadastro.");
            }
        }
    }
}