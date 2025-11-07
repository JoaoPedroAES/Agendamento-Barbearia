using barbearia.api.Data;
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

        public async Task<Service> CreateServiceAsync(Service service)
        {
            _context.Services.Add(service);
            await _context.SaveChangesAsync();
            return service; // O ID será preenchido pelo EF Core
        }

        public async Task<bool> UpdateServiceAsync(int id, Service serviceInput)
        {
            var existingService = await _context.Services.FindAsync(id);
            if (existingService == null)
            {
                return false; // Não encontrado
            }

            // Atualiza apenas as propriedades permitidas (evita overposting)
            existingService.Name = serviceInput.Name;
            existingService.Description = serviceInput.Description;
            existingService.Price = serviceInput.Price;
            existingService.DurationInMinutes = serviceInput.DurationInMinutes;

            // Marca a entidade como modificada (alternativa ao Attach/Entry)
            _context.Services.Update(existingService);

            await _context.SaveChangesAsync();
            return true; // Sucesso
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
