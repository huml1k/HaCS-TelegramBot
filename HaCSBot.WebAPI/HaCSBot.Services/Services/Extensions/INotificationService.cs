using static HaCSBot.Contracts.DTOs.DTOs;

namespace HaCSBot.Services.Services.Extensions
{
    public interface INotificationService 
    {
        public Task<Guid> CreateNotificationAsync(CreateNotificationDto dto, Guid adminId);
        public Task SendNotificationToBuildingAsync(Guid notificationId);
        public Task SendScheduledNotificationsAsync();
        public Task<List<NotificationDto>> GetUserNotificationsAsync(long telegramId, int page = 1);
        public Task<List<NotificationDto>> GetUnreadNotificationsAsync(long telegramId);
        public  Task MarkAsReadAsync(Guid notificationId, long telegramId);
        public Task<List<NotificationAdminDto>> GetAllForAdminAsync(Guid? buildingId = null);
        public Task RepeatToUnsentAsync(Guid notificationId);

    }
}
