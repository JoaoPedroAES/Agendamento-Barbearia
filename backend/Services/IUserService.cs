using barbearia.api.Dtos;
using barbearia.api.Models;
using Microsoft.AspNetCore.Identity;

namespace barbearia.api.Services
{
    public interface IUserService
    {
        // Registra um novo cliente no sistema com base nos dados fornecidos no DTO
        // Retorna o usuário criado ou lança exceções em caso de erro
        Task<ApplicationUser> RegisterCustomerAsync(RegisterCustomerDto dto);

        // Retorna o perfil completo de um usuário com base no ID fornecido
        // Inclui informações como endereço, papéis e, se aplicável, o ID de barbeiro
        Task<UserProfileDto> GetUserProfileAsync(string userId);

        // Atualiza o perfil de um usuário com base no ID e nos dados fornecidos no DTO
        // Retorna true se a atualização for bem-sucedida, ou false se o usuário não for encontrado
        Task<bool> UpdateUserProfileAsync(string userId, UpdateProfileDto dto);

        // Exclui permanentemente a conta de um usuário com base no ID fornecido
        // Anonimiza os dados do usuário e retorna o resultado da operação do Identity
        Task<IdentityResult?> DeleteUserAccountAsync(string userId);
    }
}
