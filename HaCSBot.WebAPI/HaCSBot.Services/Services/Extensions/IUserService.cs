using HaCSBot.DataBase.Models;
using static HaCSBot.Contracts.DTOs.DTOs;

namespace HaCSBot.Services.Services.Extensions
{
    public interface IUserService
    {
        Task<User?> GetCurrentUserAsync(long telegramId);
        Task<bool> IsUserAuthorizedAsync(long telegramId);
        Task<List<ApartmentInfoDto>> GetUserApartmentsAsync(long telegramId);
        Task<UserProfileDto> GetProfileAsync(long telegramId);
        Task ChangeApartmentAsync(long telegramId, Guid apartmentId);
        public Task<User?> FindByPersonalDataAsync(string firstName, string lastName, string phone);
        Task UpdateUserAsync(User user);
        Task<User?> GetByTelegramIdAsync(long telegramId);
    }
}
