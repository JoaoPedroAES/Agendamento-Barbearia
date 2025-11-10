using barbearia.api.Data;
using barbearia.api.Dtos;
using barbearia.api.Models;
using Microsoft.EntityFrameworkCore;

namespace barbearia.api.Services
{
    public class WorkScheduleService : IWorkScheduleService
    {
        private readonly AppDbContext _context;

        public WorkScheduleService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<WorkSchedule>> GetScheduleByBarberIdAsync(int barberId)
        {
            // Inclui os dados do barbeiro e sua conta para retorno completo, se necessário
            return await _context.WorkSchedules
                .Where(s => s.BarberId == barberId)
                .Include(s => s.Barber)
                    .ThenInclude(b => b.UserAccount)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task SetBatchWorkScheduleAsync(List<SetWorkScheduleDto> scheduleList)
        {
            if (scheduleList == null || !scheduleList.Any())
            {
                // Pode lançar exceção ou apenas retornar, dependendo da regra de negócio
                // throw new ArgumentException("A lista de horários não pode ser vazia.");
                return;
            }

            var barberId = scheduleList.First().BarberId; // Assume que todos são do mesmo barbeiro

            // Busca todos os horários existentes para este barbeiro de uma vez
            var existingSchedules = await _context.WorkSchedules
                                        .Where(s => s.BarberId == barberId)
                                        .ToListAsync();

            var schedulesToAdd = new List<WorkSchedule>();
            var schedulesToUpdate = new List<WorkSchedule>();

            foreach (var dto in scheduleList)
            {
                // Validação básica (pode ser mais robusta)
                if (dto.StartTime >= dto.EndTime || dto.BreakStartTime >= dto.BreakEndTime)
                {
                    throw new ArgumentException($"Horários inválidos para {dto.DayOfWeek}.");
                }

                var existing = existingSchedules.FirstOrDefault(s => s.DayOfWeek == dto.DayOfWeek);
                if (existing != null)
                {
                    // Atualiza os dados do horário existente
                    existing.StartTime = dto.StartTime;
                    existing.EndTime = dto.EndTime;
                    existing.BreakStartTime = dto.BreakStartTime;
                    existing.BreakEndTime = dto.BreakEndTime;
                    schedulesToUpdate.Add(existing); // Adiciona à lista para Update
                }
                else
                {
                    // Cria um novo registro de horário
                    var newSchedule = new WorkSchedule
                    {
                        BarberId = dto.BarberId,
                        DayOfWeek = dto.DayOfWeek,
                        StartTime = dto.StartTime,
                        EndTime = dto.EndTime,
                        BreakStartTime = dto.BreakStartTime,
                        BreakEndTime = dto.BreakEndTime
                    };
                    schedulesToAdd.Add(newSchedule); // Adiciona à lista para Add
                }
            }

            // Remove os dias que existiam antes mas não vieram na nova lista (marcou como inativo)
            var daysToRemove = existingSchedules.Where(es => !scheduleList.Any(dto => dto.DayOfWeek == es.DayOfWeek)).ToList();
            if (daysToRemove.Any())
            {
                _context.WorkSchedules.RemoveRange(daysToRemove);
            }

            // Aplica as operações em lote
            if (schedulesToAdd.Any())
            {
                _context.WorkSchedules.AddRange(schedulesToAdd);
            }
            if (schedulesToUpdate.Any())
            {
                _context.WorkSchedules.UpdateRange(schedulesToUpdate);
            }


            await _context.SaveChangesAsync();
        }
    }
}