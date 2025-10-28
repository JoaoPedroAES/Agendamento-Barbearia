using barbearia.api.Data;
using barbearia.api.Dtos;
using barbearia.api.Models;
using barbearia.api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace barbearia.api.Controllers
{
    [Route("api/appointments")]
    [ApiController]
    [Authorize]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        // Cliente: Criar um novo agendamento
        [HttpPost]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentDto dto)
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (customerId == null) return Unauthorized();

            try
            {
                var appointment = await _appointmentService.CreateAppointmentAsync(dto, customerId);
                // Retorna 201 Created com o objeto criado
                return CreatedAtAction(nameof(GetMyAppointments), appointment);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex) // Captura erro de slot ocupado
            {
                return Conflict(ex.Message); // Retorna 409 Conflict
            }
            catch (Exception ex)
            {
                // Logar erro ex
                return StatusCode(500, "Erro interno ao criar agendamento.");
            }
        }

        // Cliente: Ver meus agendamentos 
        [HttpGet("my-appointments")]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> GetMyAppointments()
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (customerId == null) return Unauthorized();

            var appointments = await _appointmentService.GetMyAppointmentsAsync(customerId);
            return Ok(appointments);
        }

        // Cliente: Cancelar um agendamento 
        [HttpPut("{id:int}/cancel")]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> CancelAppointment(int id)
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (customerId == null) return Unauthorized();

            var success = await _appointmentService.CancelAppointmentAsync(id, customerId);

            if (!success)
            {
                // Pode ser NotFound ou Forbidden dependendo da lógica do serviço
                return NotFound("Agendamento não encontrado ou não pertence ao usuário.");
            }
            return NoContent(); // Retorna 204 No Content para sucesso
        }

        // Admin/Barbeiro: Ver agenda [cite: 48]
        [HttpGet("agenda")]
        [Authorize(Roles = "Admin,Barbeiro")]
        public async Task<IActionResult> GetAgenda([FromQuery] DateTime date)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isBarber = User.IsInRole("Barbeiro");

            var appointments = await _appointmentService.GetAgendaAsync(date, userId, isBarber);
            return Ok(appointments);
        }
    }
}
