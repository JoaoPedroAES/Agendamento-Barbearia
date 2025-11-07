using barbearia.api.Models;

namespace barbearia.api.Services
{
    public interface IServicesService
    {
        Task<IEnumerable<Service>> GetAllServicesAsync();
        Task<Service?> GetServiceByIdAsync(int id);
        Task<Service> CreateServiceAsync(Service service);
        Task<bool> UpdateServiceAsync(int id, Service serviceInput);
        Task<bool> DeleteServiceAsync(int id);
    }
}
