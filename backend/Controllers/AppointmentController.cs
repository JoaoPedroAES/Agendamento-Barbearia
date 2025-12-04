using barbearia.api.Dtos;
using barbearia.api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace barbearia.api.Controllers
{
    [Route("api/appointments")]
    [ApiController]
    [Authorize]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        // Construtor que injeta o serviço de agendamentos
        public AppointmentController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        // Cliente: Criar um novo agendamento
        [HttpPost]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentDto dto)
        {
            // Obtém o ID do cliente logado a partir do token
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (customerId == null) return Unauthorized(); // Retorna 401 se o cliente não estiver autenticado

            try
            {
                // Chama o serviço para criar o agendamento
                var appointment = await _appointmentService.CreateAppointmentAsync(dto, customerId);
                // Retorna 201 Created com o objeto criado
                return CreatedAtAction(nameof(GetMyAppointments), appointment);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message); // Retorna 400 se houver erro de validação
            }
            catch (InvalidOperationException ex) // Captura erro de slot ocupado
            {
                return Conflict(ex.Message); // Retorna 409 Conflict se o horário estiver ocupado
            }
            catch (Exception ex)
            {
                // Loga o erro e retorna 500 para erros internos
                return StatusCode(500, "Erro interno ao criar agendamento.");
            }
        }

        // Cliente: Ver meus agendamentos
        [HttpGet("my-appointments")]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> GetMyAppointments()
        {
            // Obtém o ID do cliente logado a partir do token
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (customerId == null) return Unauthorized(); // Retorna 401 se o cliente não estiver autenticado

            // Chama o serviço para buscar os agendamentos do cliente
            var appointments = await _appointmentService.GetMyAppointmentsAsync(customerId);
            return Ok(appointments); // Retorna 200 com a lista de agendamentos
        }

        // Cliente: Cancelar um agendamento
        [HttpPut("{id:int}/cancel")]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> CancelAppointment(int id)
        {
            // Obtém o ID do cliente logado a partir do token
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (customerId == null) return Unauthorized(); // Retorna 401 se o cliente não estiver autenticado

            // Chama o serviço para cancelar o agendamento
            var success = await _appointmentService.CancelAppointmentAsync(id, customerId);

            if (!success)
            {
                // Retorna 404 se o agendamento não for encontrado ou não pertencer ao cliente
                return NotFound("Agendamento não encontrado ou não pertence ao usuário.");
            }
            return NoContent(); // Retorna 204 No Content para sucesso
        }

        // Admin/Barbeiro: Ver agenda
        [HttpGet("agenda")]
        [Authorize(Roles = "Admin,Barbeiro")]
        public async Task<IActionResult> GetAgenda([FromQuery] DateTime date)
        {
            // Obtém o ID do usuário logado e verifica se é barbeiro
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isBarber = User.IsInRole("Barbeiro");

            // Chama o serviço para buscar a agenda do dia
            var appointments = await _appointmentService.GetAgendaAsync(date, userId, isBarber);
            return Ok(appointments); // Retorna 200 com a lista de agendamentos
        }
    }
}