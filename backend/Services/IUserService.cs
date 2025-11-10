using barbearia.api.Dtos;
using barbearia.api.Models;
using Microsoft.AspNetCore.Identity;

namespace barbearia.api.Services
{
    public interface IUserService
    {
        Task<ApplicationUser> RegisterCustomerAsync(RegisterCustomerDto dto);
        Task<UserProfileDto> GetUserProfileAsync(string userId);
        Task<bool> UpdateUserProfileAsync(string userId, UpdateProfileDto dto);
        Task<IdentityResult?> DeleteUserAccountAsync(string userId);
    }
}
