using HaCSBot.DataBase.Models;
using HaCSBot.DataBase.Repositories.Extensions;
using Microsoft.EntityFrameworkCore;

namespace HaCSBot.DataBase.Repositories
{
    public class NotificationDeliveryRepository : INotificationDeliveryRepository
    {
        private readonly MyApplicationDbContext _context;

        public NotificationDeliveryRepository(MyApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(NotificationDelivery entity)
        {
            await _context.NotificationDeliveries.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var result = await GetByIdAsync(id);
            if (result != null)
            {
                _context.NotificationDeliveries.Remove(result);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<NotificationDelivery>> GetAllAsync()
        {
            return await _context.NotificationDeliveries
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<NotificationDelivery?> GetByIdAsync(Guid id)
        {
            return await _context.NotificationDeliveries
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<NotificationDelivery>> GetByTelegramUserIdAsync(long telegramUserId)
        {
            return await _context.NotificationDeliveries
                .AsNoTracking()
                .Where(x => x.TelegramUserId == telegramUserId)
                .ToListAsync();
        }

        public async Task UpdateAsync(NotificationDelivery entity)
        {
            _context.NotificationDeliveries.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}
