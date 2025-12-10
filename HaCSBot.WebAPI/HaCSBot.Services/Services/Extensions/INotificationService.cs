using HaCSBot.DataBase.Enums;
using HaCSBot.DataBase.Models;

namespace HaCSBot.Services.Services.Extensions
{
    public interface INotificationService 
    {
		public Task AddAsync(Notification notification);
		public Task UpdateAsync(Notification notification);
		public Task<Notification> GetByIdAsync(Guid id);
		public Task<Notification> GetByIdWithDetailsAsync(Guid id); // с Include: Building, CreatedByUser, Attachments, Deliveries
		public Task<List<Notification>> GetLatestForUserAsync(long telegramId, int count = 20); // последние уведомления для жильца
		public Task<List<Notification>> GetUnreadForUserAsync(long telegramId); // непрочитанные (Deliveries.ReadDate == null)
		public Task<List<Notification>> GetAllForBuildingAsync(Guid buildingId, int page = 1, int pageSize = 20); // история дома
		public Task<List<Notification>> GetScheduledAsync(DateTime fromUtc); // отложенные уведомления (ScheduledSendDate != null)
		public Task<List<Notification>> GetAllWithDeliveriesAsync(); // для админа: все уведомления + статусы доставки
		public Task MarkAsReadAsync(Guid notificationId, long telegramId); // ставит ReadDate в Delivery
		public Task<NotificationDelivery?> GetDeliveryStatusAsync(Guid notificationId, long telegramId);
		public Task DeleteAsync(Guid id); 
		public Task<bool> ExistsAsync(Guid id);

	}
}
