using barbearia.api.Dtos;
using barbearia.api.Models;

namespace barbearia.api.Services
{
    public interface IServicesService
    {
        // Retorna todos os serviços cadastrados no sistema
        Task<IEnumerable<Service>> GetAllServicesAsync();

        // Retorna os dados de um serviço específico com base no ID fornecido
        Task<Service?> GetServiceByIdAsync(int id);

        // Cria um novo serviço com base nos dados fornecidos no DTO
        Task<Service> CreateServiceAsync(CreateServiceDto dto);

        // Atualiza os dados de um serviço existente com base no ID e nos dados fornecidos no DTO
        Task<Service> UpdateServiceAsync(int id, UpdateServiceDto dto);

        // Exclui um serviço com base no ID fornecido
        // Retorna true se a exclusão for bem-sucedida, ou false se o serviço não for encontrado
        Task<bool> DeleteServiceAsync(int id);
    }
}
