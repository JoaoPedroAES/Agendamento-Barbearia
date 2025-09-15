using barbearia.api.Data;
using barbearia.api.Dtos;
using barbearia.api.Models;
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
        private readonly AppDbContext _context;
        public AppointmentController(AppDbContext context) { _context = context; }

        // Cliente: Criar um novo agendamento
        [HttpPost]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentDto dto)
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (customerId == null) return Unauthorized();

            // 1. Buscar os serviços e calcular total
            var services = new List<Service>();
            decimal totalPrice = 0;
            int totalDuration = 0;
            foreach (var serviceId in dto.ServiceIds)
            {
                var service = await _context.Services.FindAsync(serviceId);
                if (service == null) return NotFound($"Serviço {serviceId} não encontrado.");
                services.Add(service);
                totalPrice += service.Price;
                totalDuration += service.DurationInMinutes;
            }

            var startDateTimeUtc = DateTime.SpecifyKind(dto.StartDateTime, DateTimeKind.Utc);

            var appointment = new Appointment
            {
                CustomerId = customerId,
                BarberId = dto.BarberId,
                StartDateTime = startDateTimeUtc, 
                EndDateTime = startDateTimeUtc.AddMinutes(totalDuration),
                Status = AppointmentStatus.Scheduled,
                TotalPrice = totalPrice,
                Services = services
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            

            return Ok(appointment);
        }

        // Cliente: Ver meus agendamentos 
        [HttpGet("my-appointments")]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> GetMyAppointments()
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var appointments = await _context.Appointments
                .Where(a => a.CustomerId == customerId)
                .Include(a => a.Barber)
                .Include(a => a.Services)
                .OrderByDescending(a => a.StartDateTime)
                .ToListAsync();

            return Ok(appointments);
        }

        // Cliente: Cancelar um agendamento 
        [HttpPut("{id:int}/cancel")]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> CancelAppointment(int id)
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var appointment = await _context.Appointments.FindAsync(id);

            if (appointment == null) return NotFound();
            if (appointment.CustomerId != customerId) return Forbid(); // Não é dono

            appointment.Status = AppointmentStatus.CancelledByCustomer;
            await _context.SaveChangesAsync();
            return Ok();
        }

        // Admin/Barbeiro: Ver agenda [cite: 48]
        [HttpGet("agenda")]
        [Authorize(Roles = "Admin,Barbeiro")]
        public async Task<IActionResult> GetAgenda([FromQuery] DateTime date)
        {
            // (Se for barbeiro, filtrar para ver apenas os seus)
            var appointments = await _context.Appointments
                .Where(a => a.StartDateTime.Date == date.Date)
                .Include(a => a.Customer)
                .Include(a => a.Services)
                .ToListAsync();

            return Ok(appointments);
        }
    }
}
