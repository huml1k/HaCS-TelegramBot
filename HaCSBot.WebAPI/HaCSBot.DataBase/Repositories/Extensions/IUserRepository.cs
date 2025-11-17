using HaCSBot.DataBase.Enums;
using HaCSBot.DataBase.Models;

namespace HaCSBot.DataBase.Repositories.Extensions
{
    public interface IUserRepository : IGenericRepository<User>
    {
        public Task<User> GetByTelegramIdAsync(long telegramId);
        public Task<IEnumerable<User>> GetByRoleAsync(Roles role);
    }
}
