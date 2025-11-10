using barbearia.api.Dtos;
using barbearia.api.Models;

namespace barbearia.api.Services
{
    public interface IServicesService
    {
        Task<IEnumerable<Service>> GetAllServicesAsync();
        Task<Service?> GetServiceByIdAsync(int id);
        Task<Service> CreateServiceAsync(CreateServiceDto dto);
        Task<Service> UpdateServiceAsync(int id, UpdateServiceDto dto);
        Task<bool> DeleteServiceAsync(int id);
    }
}
