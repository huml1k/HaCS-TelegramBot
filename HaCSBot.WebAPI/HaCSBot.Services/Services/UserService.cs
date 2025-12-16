using AutoMapper;
using HaCSBot.Contracts.DTOs;
using HaCSBot.DataBase.Enums;
using HaCSBot.DataBase.Models;
using HaCSBot.DataBase.Repositories.Extensions;
using HaCSBot.Services.Services.Extensions;

namespace HaCSBot.Services.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IApartmentRepository _apartmentRepository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository,
            IApartmentRepository apartmentRepository,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _apartmentRepository = apartmentRepository;
			_mapper = mapper;
        }

		public async Task<User?> GetCurrentUserAsync(long telegramId)
		{
			return await _userRepository.GetByTelegramIdAsync(telegramId);
		}

		public async Task<bool> IsUserAuthorizedAsync(long telegramId)
		{
			var user = await _userRepository.GetByTelegramIdAsync(telegramId);
			return user?.IsAuthorizedInBot ?? false;
		}

		public async Task<AuthorizationResultDto> LoginAsync(UserLoginDto dto)
		{
			try
			{
				// Ищем пользователя по телефону
				var user = await _userRepository.GetByPhoneAsync(dto.Phone);
				if (user == null)
				{
					return new AuthorizationResultDto
					{
						Success = false
					};
				}

				// Проверяем, не привязан ли уже этот TelegramId к другому пользователю
				var existingUserWithSameTelegram = await _userRepository.GetByTelegramIdAsync(dto.TelegramId);
				if (existingUserWithSameTelegram != null && existingUserWithSameTelegram.Id != user.Id)
				{
					return new AuthorizationResultDto
					{
						Success = false
					};
				}

				// Обновляем данные для авторизации через Telegram
				user.TelegramId = dto.TelegramId;
				user.IsAuthorizedInBot = true;
				user.LastAuthorizationDate = DateTime.UtcNow;

				await _userRepository.UpdateAsync(user);

				// Маппим в DTO
				var userDto = _mapper.Map<UserDto>(user);

				return new AuthorizationResultDto
				{
					Success = true,
					UserId = user.Id
				};
			}
			catch (Exception ex)
			{
				return new AuthorizationResultDto
				{
					Success = false
				};
			}
		}

		public async Task<List<ApartmentDto>> GetUserApartmentsAsync(long telegramId)
		{
			var user = await _userRepository.GetByTelegramIdAsync(telegramId);
			if (user == null) return new List<ApartmentDto>();

			var apartments = await _apartmentRepository.GetByUserIdAsync(user.Id);
			return _mapper.Map<List<ApartmentDto>>(apartments);
		}

		public async Task<UserProfileDto> GetProfileAsync(long telegramId)
		{
			var user = await _userRepository.GetByTelegramIdAsync(telegramId);
			if (user == null) return null;

			return _mapper.Map<UserProfileDto>(user);
		}

		public async Task<User?> FindByPhoneAsync(string phone)
		{
			return await _userRepository.GetByPhoneAsync(phone);
		}

		//private static string NormalizePhone(string phone)
		//{
		//	if (string.IsNullOrWhiteSpace(phone)) return "";
		//	return new string(phone.Where(char.IsDigit).ToArray());
		//}

		public async Task<User> GetByTelegramIdAsync(long telegramId)
		{
			return await _userRepository.GetByTelegramIdAsync(telegramId);
		}

		public async Task UpdateUserAsync(User user)
		{
			await _userRepository.UpdateAsync(user);
		}

		public async Task<UserDto> GetUserDtoAsync(long telegramId)
		{
			var user = await _userRepository.GetByTelegramIdAsync(telegramId);
			if (user == null) return null;

			return _mapper.Map<UserDto>(user);
		}

		public async Task LogoutAsync(long telegramId)
		{
			var user = await _userRepository.GetByTelegramIdAsync(telegramId);
			if (user != null)
			{
				user.IsAuthorizedInBot = false;
				user.LastAuthorizationDate = DateTime.UtcNow;
				await _userRepository.UpdateAsync(user);
			}
		}
	}
}

