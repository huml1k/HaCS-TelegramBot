using HaCSBot.DataBase.Enums;
using HaCSBot.DataBase.Models;
using HaCSBot.DataBase.Repositories.Extensions;
using Microsoft.EntityFrameworkCore;

namespace HaCSBot.DataBase.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly MyApplicationDbContext _context;
        public UserRepository(MyApplicationDbContext context)
        {
            _context = context;
        }

		public async Task<bool> ExistsByTelegramIdAsync(long telegramId)
		{
			return await _context.Users.AnyAsync(u => u.TelegramId == telegramId);
		}

		public async Task<IEnumerable<User>> GetAllAsync()
		{
            return await _context.Users.ToListAsync();
        }

		public async Task<User?> GetByIdAsync(Guid id)
		{
            return await _context.Users.FindAsync(id);
        }

		public async Task<User?> GetByPhoneAsync(string phone)
		{
            return await _context.Users.FirstOrDefaultAsync(u => u.Phone == phone);
		}

		public async Task<IEnumerable<User>> GetByRoleAsync(Roles role)
        {
            return await _context.Users.Where(u => u.Role == role).ToListAsync();
        }

        public async Task<User?> GetByTelegramIdAsync(long telegramId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.TelegramId == telegramId);
        }

        public async Task UpdateAsync(User entity)
		{
            _context.Users.Update(entity);
            await _context.SaveChangesAsync();
        }
	}
}
