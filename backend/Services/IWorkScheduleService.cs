using barbearia.api.Dtos;
using barbearia.api.Models;

namespace barbearia.api.Services
{
    public interface IWorkScheduleService
    {
        // Retorna a lista de horários de trabalho de um barbeiro específico com base no ID fornecido
        Task<IEnumerable<WorkSchedule>> GetScheduleByBarberIdAsync(int barberId);

        // Define ou atualiza em lote os horários de trabalho de um barbeiro com base na lista de horários fornecida
        Task SetBatchWorkScheduleAsync(List<SetWorkScheduleDto> scheduleList);
    }
}