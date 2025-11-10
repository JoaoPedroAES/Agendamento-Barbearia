using barbearia.api.Data;
using barbearia.api.Dtos;
using barbearia.api.Models;
using Microsoft.EntityFrameworkCore;

namespace barbearia.api.Services
{
    public class ServicesService : IServicesService
    {
        private readonly AppDbContext _context;

        public ServicesService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Service>> GetAllServicesAsync()
        {
            return await _context.Services.AsNoTracking().ToListAsync(); // AsNoTracking para consultas de leitura
        }

        public async Task<Service?> GetServiceByIdAsync(int id)
        {
            return await _context.Services.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Service> CreateServiceAsync(CreateServiceDto dto)
        {
            // Mapeia do DTO para o Modelo
            var service = new Service
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                DurationInMinutes = dto.DurationInMinutes
            };

            _context.Services.Add(service);
            await _context.SaveChangesAsync();
            return service; // Retorna o novo serviço criado
        }

        public async Task<Service> UpdateServiceAsync(int id, UpdateServiceDto dto)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                return null; // Retorna nulo se não encontrar
            }

            // Mapeia os dados do DTO para o modelo existente
            service.Name = dto.Name;
            service.Description = dto.Description;
            service.Price = dto.Price;
            service.DurationInMinutes = dto.DurationInMinutes;

            await _context.SaveChangesAsync();
            return service;
        }

        public async Task<bool> DeleteServiceAsync(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                return false; // Não encontrado
            }

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
            return true; // Sucesso
        }
    }
}
