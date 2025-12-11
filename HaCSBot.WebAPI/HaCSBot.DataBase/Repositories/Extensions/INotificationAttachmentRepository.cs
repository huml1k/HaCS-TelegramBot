using HaCSBot.DataBase.Enums;
using HaCSBot.DataBase.Models;

namespace HaCSBot.DataBase.Repositories.Extensions
{
    public interface INotificationAttachmentRepository
    {
        public Task<NotificationAttachment?> GetByIdAsync(Guid id);
        public Task<IEnumerable<NotificationAttachment>> GetByTypeAsync(AttachmentType attachmentType);
        public Task<IEnumerable<NotificationAttachment>> GetByTelegramFileIdAsync(string telegramFileId);
        public Task<IEnumerable<NotificationAttachment>> GetAllAsync();
        public Task AddAsync(NotificationAttachment entity);
        public Task UpdateAsync(NotificationAttachment entity);
        public Task DeleteAsync(Guid id);
    }
}
