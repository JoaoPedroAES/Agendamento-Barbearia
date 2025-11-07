namespace barbearia.api.Services
{
    public interface IAvailabilityService
    {
        // Método que calcula os slots disponíveis
        Task<List<TimeSpan>> GetAvailableSlotsAsync(int barberId, List<int> serviceIds, DateTime date);
    }
}
