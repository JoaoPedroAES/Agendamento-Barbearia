using barbearia.api.Dtos;

namespace barbearia.api.Services
{
    public interface IBarberService
    {
        
        Task<IEnumerable<BarberProfileDto>> GetActiveBarbersAsync();
        Task<BarberProfileDto> GetBarberByIdAsync(int barberId);
        Task<bool> AcceptTermsAsync(int barberId);
        Task<BarberProfileDto> CreateBarberAsync(CreateBarberDto dto);
        Task<bool> UpdateBarberAsync(int barberId, UpdateBarberDto dto);
        Task<bool> DeactivateBarberAsync(int barberId);
    }
}
