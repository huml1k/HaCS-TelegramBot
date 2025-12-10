using HaCSBot.DataBase.Enums;
using HaCSBot.DataBase.Models;

namespace HaCSBot.Services.Services.Extensions
{
    public interface INotificationDeliveryService
    {
        public Task<NotificationDelivery> GetByIdAsync(Guid id);
        public Task<IEnumerable<NotificationDelivery>> GetByTelegramUserIdAsync(long telegramUserId);
        public Task<IEnumerable<NotificationDelivery>> GetAllAsync();
        public Task AddAsync(NotificationDelivery entity);
        public Task UpdateAsync(NotificationDelivery entity);
        public Task DeleteAsync(Guid id);
    }
}
