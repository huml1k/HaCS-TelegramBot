using HaCSBot.DataBase.Models;
using static HaCSBot.Contracts.DTOs.DTOs;

namespace HaCSBot.Services.Services.Extensions
{
    public interface IUserService
    {
        Task<User?> GetCurrentUserAsync(long telegramId);
        Task<bool> IsUserAuthorizedAsync(long telegramId);
        //Task<AuthorizationResult> RegisterOrLoginAsync(UserRegistrationDto dto);
        Task<List<ApartmentInfoDto>> GetUserApartmentsAsync(long telegramId);
        Task<UserProfileDto> GetProfileAsync(long telegramId);
        Task ChangeApartmentAsync(long telegramId, Guid apartmentId);
    }
}
