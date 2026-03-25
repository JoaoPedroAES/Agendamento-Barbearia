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

        // Retorna apenas os serviços ATIVOS (IsActive == true)
        public async Task<IEnumerable<Service>> GetAllServicesAsync()
        {
            return await _context.Services
                .AsNoTracking()
                .Where(s => s.IsActive) // <--- FILTRO ADICIONADO
                .ToListAsync();
        }

        public async Task<Service?> GetServiceByIdAsync(int id)
        {
            return await _context.Services
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id && s.IsActive); // <--- GARANTE QUE SÓ BUSCA ATIVOS
        }

        public async Task<Service> CreateServiceAsync(CreateServiceDto dto)
        {
            var service = new Service
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                DurationInMinutes = dto.DurationInMinutes,
                IsActive = true // <--- GARANTE QUE NASCE ATIVO
            };

            _context.Services.Add(service);
            await _context.SaveChangesAsync();
            return service;
        }

        public async Task<Service> UpdateServiceAsync(int id, UpdateServiceDto dto)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null || !service.IsActive) // <--- SÓ ATUALIZA SE ESTIVER ATIVO
            {
                return null;
            }

            service.Name = dto.Name;
            service.Description = dto.Description;
            service.Price = dto.Price;
            service.DurationInMinutes = dto.DurationInMinutes;

            await _context.SaveChangesAsync();
            return service;
        }

        // SOFT DELETE: Agora apenas desativa o serviço
        public async Task<bool> DeleteServiceAsync(int id)
        {
            var service = await _context.Services.FindAsync(id);

            // Se não encontrar ou já estiver inativo, não faz nada
            if (service == null || !service.IsActive)
            {
                return false;
            }

            
            service.IsActive = false;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}