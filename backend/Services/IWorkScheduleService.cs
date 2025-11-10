using barbearia.api.Dtos;
using barbearia.api.Models;

namespace barbearia.api.Services
{
    public interface IWorkScheduleService
    {
        Task<IEnumerable<WorkSchedule>> GetScheduleByBarberIdAsync(int barberId);
        Task SetBatchWorkScheduleAsync(List<SetWorkScheduleDto> scheduleList);
    }
}