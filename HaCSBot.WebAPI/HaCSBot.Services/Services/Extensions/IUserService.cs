using HaCSBot.Contracts.DTOs;
using HaCSBot.DataBase.Models;

namespace HaCSBot.Services.Services.Extensions
{
	public interface IUserService
	{
		Task<User?> GetCurrentUserAsync(long telegramId);
		Task<bool> IsUserAuthorizedAsync(long telegramId);
		Task<AuthorizationResultDto> LoginAsync(UserLoginDto dto);
		Task<List<ApartmentDto>> GetUserApartmentsAsync(long telegramId);
		Task<UserProfileDto> GetProfileAsync(long telegramId);
		Task<User?> FindByPhoneAsync(string phone);
		Task<User?> GetByTelegramIdAsync(long telegramId);
		Task UpdateUserAsync(User user);
		Task<UserDto> GetUserDtoAsync(long telegramId);

		Task LogoutAsync(long telegramId);
	}
}
