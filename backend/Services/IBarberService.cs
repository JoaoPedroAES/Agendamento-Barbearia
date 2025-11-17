using barbearia.api.Dtos;

namespace barbearia.api.Services
{
    public interface IBarberService
    {
        // Retorna uma lista de barbeiros ativos (aqueles que estão disponíveis para agendamentos)
        Task<IEnumerable<BarberProfileDto>> GetActiveBarbersAsync();

        // Retorna os dados de um barbeiro específico com base no ID fornecido
        Task<BarberProfileDto> GetBarberByIdAsync(int barberId);

        // Marca que o barbeiro aceitou os termos de uso
        Task<bool> AcceptTermsAsync(int barberId);

        // Cria um novo barbeiro com base nos dados fornecidos no DTO
        Task<BarberProfileDto> CreateBarberAsync(CreateBarberDto dto);

        // Atualiza os dados de um barbeiro existente com base no ID e nos dados fornecidos no DTO
        Task<bool> UpdateBarberAsync(int barberId, UpdateBarberDto dto);

        // Desativa um barbeiro, tornando-o indisponível para agendamentos
        Task<bool> DeactivateBarberAsync(int barberId);
    }
}
