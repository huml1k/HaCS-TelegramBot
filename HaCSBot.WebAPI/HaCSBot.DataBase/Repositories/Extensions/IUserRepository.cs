using HaCSBot.DataBase.Enums;
using HaCSBot.DataBase.Models;
using Microsoft.EntityFrameworkCore;

namespace HaCSBot.DataBase.Repositories.Extensions
{
    public interface IUserRepository
    {
        public Task<User> GetByTelegramIdAsync(long telegramId);
		public Task<bool> ExistsByTelegramIdAsync(long telegramId);
		public Task<IEnumerable<User>> GetByRoleAsync(Roles role);
		public Task<IEnumerable<User>> GetAllAsync();
		public Task<User> GetByIdAsync(Guid id);
		public Task UpdateAsync(User entity);
		public Task<User> GetByPhoneAsync(string phone);

	}
}
