using barbearia.api.Data;
using barbearia.api.Dtos;
using barbearia.api.Models;
using Microsoft.EntityFrameworkCore;

namespace barbearia.api.Services
{
    public class ServicesService : IServicesService
    {
        private readonly AppDbContext _context;

        // Construtor que recebe o contexto do banco de dados
        public ServicesService(AppDbContext context)
        {
            _context = context;
        }

        // Retorna todos os serviços cadastrados no banco de dados
        public async Task<IEnumerable<Service>> GetAllServicesAsync()
        {
            return await _context.Services.AsNoTracking().ToListAsync(); // AsNoTracking para melhorar a performance em consultas de leitura
        }

        // Retorna um serviço específico com base no ID fornecido
        public async Task<Service?> GetServiceByIdAsync(int id)
        {
            return await _context.Services.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        }

        // Cria um novo serviço com base nos dados fornecidos no DTO
        public async Task<Service> CreateServiceAsync(CreateServiceDto dto)
        {
            // Mapeia os dados do DTO para o modelo de serviço
            var service = new Service
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                DurationInMinutes = dto.DurationInMinutes
            };

            // Adiciona o novo serviço ao banco de dados
            _context.Services.Add(service);
            await _context.SaveChangesAsync(); // Salva as alterações no banco
            return service; // Retorna o serviço criado
        }

        // Atualiza os dados de um serviço existente com base no ID e no DTO fornecido
        public async Task<Service> UpdateServiceAsync(int id, UpdateServiceDto dto)
        {
            // Busca o serviço pelo ID
            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                return null; // Retorna nulo se o serviço não for encontrado
            }

            // Atualiza os dados do serviço com base no DTO
            service.Name = dto.Name;
            service.Description = dto.Description;
            service.Price = dto.Price;
            service.DurationInMinutes = dto.DurationInMinutes;

            // Salva as alterações no banco de dados
            await _context.SaveChangesAsync();
            return service;
        }

        // Exclui um serviço com base no ID fornecido
        public async Task<bool> DeleteServiceAsync(int id)
        {
            // Busca o serviço pelo ID
            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                return false; // Retorna false se o serviço não for encontrado
            }

            // Remove o serviço do banco de dados
            _context.Services.Remove(service);
            await _context.SaveChangesAsync(); // Salva as alterações no banco
            return true; // Retorna true se a exclusão for bem-sucedida
        }
    }
}
