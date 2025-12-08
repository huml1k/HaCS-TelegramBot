using HaCSBot.DataBase.Enums;
using HaCSBot.DataBase.Models;
using Microsoft.EntityFrameworkCore;

namespace HaCSBot.DataBase.Repositories.Extensions
{
    public interface IUserRepository : IGenericRepository<User>
    {
        public Task<User> GetByTelegramIdAsync(long telegramId);
        public Task<IEnumerable<User>> GetByRoleAsync(Roles role);
		public Task Create(User entity);
		public Task Delete(Guid id);
		public Task<IEnumerable<User>> GetAll();
		public Task<User> GetById(Guid id);
		public Task Update(User entity);

	}
}
