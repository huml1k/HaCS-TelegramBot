using AutoMapper;
using HaCSBot.Contracts.Contracts;
using HaCSBot.DataBase.Enums;
using HaCSBot.DataBase.Models;
using HaCSBot.DataBase.Repositories.Extensions;
using HaCSBot.Services.Services.Extensions;

namespace HaCSBot.Services.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserService(
            IUserRepository userRepository,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task DeleteUserAsync(Guid id)
        {
            await _userRepository.Delete(id);
		}

		public async Task<bool> RegisterAsync(UserRegistrationContract request)
        {
            if (request == null)
            {
                return false;
            }
            var resultRegistration = _mapper.Map<User>(request);
            await _userRepository.Create(resultRegistration);
            return true;
        }

		public async Task<User?> FindByPersonalDataAsync(string firstName, string lastName, string phone)
		{
			var normalizedPhone = NormalizePhone(phone);

			var allUsers = await _userRepository.GetAll();

			return allUsers.FirstOrDefault(u =>
				u.FirstName.Trim() == firstName.Trim() &&
				u.LastName.Trim() == lastName.Trim() &&
				NormalizePhone(u.Phone) == normalizedPhone);
		}

		private static string NormalizePhone(string phone)
		{
			if (string.IsNullOrWhiteSpace(phone)) return "";
			return phone.Replace("+", "").Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
		}

		public async Task<User?> GetByTelegramIdAsync(long telegramId)
		{
			var allUsers = await _userRepository.GetAll();
			return allUsers.FirstOrDefault(u => u.TelegramId == telegramId && u.IsAuthorizedInBot);
		}

		public async Task UpdateUserAsync(User user)
		{
			await _userRepository.Update(user);
		}
	}
}
