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

		public async Task AddAsync(Notification notification)
		{
			await _context.Notifications.AddAsync(notification);
			await _context.SaveChangesAsync();
		}

		public async Task UpdateAsync(Notification notification)
		{
			_context.Notifications.Update(notification);
			await _context.SaveChangesAsync();
		}

		public async Task<Notification?> GetByIdAsync(Guid id)
		{
			return await _context.Notifications
				.FirstOrDefaultAsync(n => n.Id == id);
		}

		public async Task<Notification?> GetByIdWithDetailsAsync(Guid id)
		{
			return await _context.Notifications
				.AsNoTracking()
				.Include(n => n.Building)
				.Include(n => n.CreatedByUser)
				.Include(n => n.Attachments)
				.Include(n => n.Deliveries)
				.FirstOrDefaultAsync(n => n.Id == id);
		}

		public async Task<List<Notification>> GetLatestForUserAsync(long telegramId, int count = 20)
		{
			return await _context.NotificationDeliveries
				.AsNoTracking()
				.Where(d => d.TelegramUserId == telegramId)
				.OrderByDescending(d => d.Notification!.CreatedDate)
				.Take(count)
				.Select(d => d.Notification!)
				.Include(n => n.Attachments)
				.Include(n => n.Building)
				.ToListAsync();
		}

		public async Task<List<Notification>> GetUnreadForUserAsync(long telegramId)
		{
			return await _context.NotificationDeliveries
				.AsNoTracking()
				.Where(d => d.TelegramUserId == telegramId && d.ReadDate == null)
				.OrderByDescending(d => d.Notification!.CreatedDate)
				.Select(d => d.Notification!)
				.Include(n => n.Attachments)
				.ToListAsync();
		}

		public async Task<List<Notification>> GetAllForBuildingAsync(Guid buildingId, int page = 1, int pageSize = 20)
		{
			return await _context.Notifications
				.AsNoTracking()
				.Where(n => n.BuildingId == buildingId)
				.OrderByDescending(n => n.CreatedDate)
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.Include(n => n.Attachments)
				.Include(n => n.CreatedByUser)
				.ToListAsync();
		}

		public async Task<List<Notification>> GetScheduledAsync(DateTime fromUtc)
		{
			return await _context.Notifications
				.Where(n => n.ScheduledSendDate != null && n.ScheduledSendDate <= fromUtc)
				.Include(n => n.Attachments)
				.Include(n => n.Deliveries)
				.ToListAsync();
		}

		public async Task<List<Notification>> GetAllWithDeliveriesAsync()
		{
			return await _context.Notifications
				.AsNoTracking()
				.Include(n => n.Deliveries)
				.Include(n => n.Building)
				.Include(n => n.CreatedByUser)
				.OrderByDescending(n => n.CreatedDate)
				.ToListAsync();
		}

		public async Task MarkAsReadAsync(Guid notificationId, long telegramId)
		{
			var delivery = await _context.NotificationDeliveries
				.FirstOrDefaultAsync(d => d.NotificationId == notificationId && d.TelegramUserId == telegramId);

			if (delivery != null && delivery.ReadDate == null)
			{
				delivery.ReadDate = DateTime.UtcNow;
				await _context.SaveChangesAsync();
			}
		}

		public async Task<NotificationDelivery?> GetDeliveryStatusAsync(Guid notificationId, long telegramId)
		{
			return await _context.NotificationDeliveries
				.FirstOrDefaultAsync(d => d.NotificationId == notificationId && d.TelegramUserId == telegramId);
		}

		public async Task DeleteAsync(Guid id)
		{
			var notification = await GetByIdAsync(id);
			if (notification != null)
			{
				_context.Notifications.Remove(notification);
				await _context.SaveChangesAsync();
			}
		}

		public async Task<bool> ExistsAsync(Guid id)
		{
			return await _context.Notifications.AnyAsync(n => n.Id == id);
		}


	}
}
