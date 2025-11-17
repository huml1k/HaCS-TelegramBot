using HaCSBot.DataBase.Enums;
using HaCSBot.DataBase.Models;
using HaCSBot.DataBase.Repositories.Extensions;
using Microsoft.EntityFrameworkCore;

namespace HaCSBot.DataBase.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private MyApplicationDbContext _context;

        public NotificationRepository(MyApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Create(Notification entity)
        {
            await _context.Notifications.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(Guid id)
        {
            var entity = await GetById(id);
            if (entity != null)
            {
                _context.Notifications.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Notification>> GetAll()
        {
            return await _context.Notifications.ToListAsync();
        }

        public async Task<IEnumerable<Notification>> GetByBuildingIdAsync(Guid buildingId)
        {
            return await _context.Notifications.Where(x => x.BuildingId == buildingId).ToListAsync();
        }

        public async Task<Notification> GetById(Guid id)
        {
            return await _context.Notifications.FindAsync(id);
        }

        public async Task<IEnumerable<Notification>> GetByTypeAsync(NotificationType type)
        {
            return await _context.Notifications.Where(x => x.Type == type).ToListAsync();
        }

        public async Task Update(Notification entity)
        {
            _context.Notifications.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}
