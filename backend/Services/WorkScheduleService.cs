using barbearia.api.Data;
using barbearia.api.Dtos;
using barbearia.api.Models;
using Microsoft.EntityFrameworkCore;

namespace barbearia.api.Services
{
    public class WorkScheduleService : IWorkScheduleService
    {
        private readonly AppDbContext _context;

        // Construtor que recebe o contexto do banco de dados
        public WorkScheduleService(AppDbContext context)
        {
            _context = context;
        }

        // Retorna os horários de trabalho de um barbeiro específico com base no ID
        public async Task<IEnumerable<WorkSchedule>> GetScheduleByBarberIdAsync(int barberId)
        {
            // Busca os horários de trabalho do barbeiro, incluindo os dados do barbeiro e sua conta
            return await _context.WorkSchedules
                .Where(s => s.BarberId == barberId)
                .Include(s => s.Barber)
                    .ThenInclude(b => b.UserAccount) // Inclui os dados da conta do barbeiro
                .AsNoTracking() // Apenas leitura, sem rastreamento
                .ToListAsync();
        }

        // Define ou atualiza em lote os horários de trabalho de um barbeiro
        public async Task SetBatchWorkScheduleAsync(List<SetWorkScheduleDto> scheduleList)
        {
            // Verifica se a lista de horários está vazia ou nula
            if (scheduleList == null || !scheduleList.Any())
            {
                return;
            }

            // Obtém o ID do barbeiro (assume que todos os horários são do mesmo barbeiro)
            var barberId = scheduleList.First().BarberId;

            // Busca todos os horários existentes para o barbeiro no banco de dados
            var existingSchedules = await _context.WorkSchedules
                                        .Where(s => s.BarberId == barberId)
                                        .ToListAsync();

            var schedulesToAdd = new List<WorkSchedule>(); // Lista para novos horários
            var schedulesToUpdate = new List<WorkSchedule>(); // Lista para horários a serem atualizados

            foreach (var dto in scheduleList)
            {
                // Valida se o horário de trabalho é válido (início deve ser antes do fim)
                if (dto.StartTime >= dto.EndTime)
                {
                    throw new ArgumentException($"Horários inválidos para {dto.DayOfWeek}.");
                }

                // Valida se o horário de pausa é válido (início deve ser antes do fim, se não for 00:00)
                if (dto.BreakStartTime >= dto.BreakEndTime && dto.BreakEndTime != TimeSpan.Zero)
                {
                    throw new ArgumentException($"Horários de pausa inválidos para {dto.DayOfWeek}.");
                }

                // Verifica se já existe um horário para o mesmo dia da semana
                var existing = existingSchedules.FirstOrDefault(s => s.DayOfWeek == dto.DayOfWeek);
                if (existing != null)
                {
                    // Atualiza os dados do horário existente
                    existing.StartTime = dto.StartTime;
                    existing.EndTime = dto.EndTime;
                    existing.BreakStartTime = dto.BreakStartTime;
                    existing.BreakEndTime = dto.BreakEndTime;
                    schedulesToUpdate.Add(existing); // Adiciona à lista de atualização
                }
                else
                {
                    // Cria um novo horário de trabalho
                    var newSchedule = new WorkSchedule
                    {
                        BarberId = dto.BarberId,
                        DayOfWeek = dto.DayOfWeek,
                        StartTime = dto.StartTime,
                        EndTime = dto.EndTime,
                        BreakStartTime = dto.BreakStartTime,
                        BreakEndTime = dto.BreakEndTime
                    };
                    schedulesToAdd.Add(newSchedule); // Adiciona à lista de criação
                }
            }

            // Remove os horários que não estão na nova lista (marcados como inativos)
            var daysToRemove = existingSchedules.Where(es => !scheduleList.Any(dto => dto.DayOfWeek == es.DayOfWeek)).ToList();
            if (daysToRemove.Any())
            {
                _context.WorkSchedules.RemoveRange(daysToRemove);
            }

            // Adiciona os novos horários ao banco de dados
            if (schedulesToAdd.Any())
            {
                _context.WorkSchedules.AddRange(schedulesToAdd);
            }

            // Atualiza os horários existentes no banco de dados
            if (schedulesToUpdate.Any())
            {
                _context.WorkSchedules.UpdateRange(schedulesToUpdate);
            }

            // Salva as alterações no banco de dados
            await _context.SaveChangesAsync();
        }
    }
}