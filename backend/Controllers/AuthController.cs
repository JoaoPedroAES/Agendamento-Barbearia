using barbearia.api.Dtos;
using barbearia.api.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace barbearia.api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        // Dependência do serviço de usuário (IUserService)
        private readonly IUserService _userService;

        // Construtor que injeta o serviço de usuário
        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        // Endpoint para registrar um novo cliente
        [HttpPost("register-customer")]
        public async Task<IActionResult> RegisterCustomer([FromBody] RegisterCustomerDto dto)
        {
            // Valida o modelo recebido (verifica se os campos obrigatórios estão preenchidos)
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Retorna 400 se o modelo for inválido
            }

            try
            {
                // Chama o serviço para registrar o cliente
                var user = await _userService.RegisterCustomerAsync(dto);

                // Retorna 201 Created com uma mensagem de sucesso
                return StatusCode(201, new { Message = $"{(dto.CreateAsAdmin ? "Administrador" : "Cliente")} cadastrado com sucesso!" });
            }
            catch (ArgumentException ex) // Captura erros de e-mail duplicado
            {
                return BadRequest(ex.Message); // Retorna 400 com a mensagem de erro
            }
            catch (InvalidOperationException ex) // Captura erros do Identity
            {
                return BadRequest(ex.Message); // Retorna 400 com a mensagem de erro
            }
            catch (Exception ex) // Captura erros genéricos
            {
                // Loga o erro para depuração
                Console.WriteLine($"Erro inesperado em RegisterCustomer: {ex}");
                // Retorna 500 para erros internos
                return StatusCode(500, "Ocorreu um erro interno durante o cadastro.");
            }
        }
    }
}