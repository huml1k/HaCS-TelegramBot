using HaCSBot.DataBase.Enums;
using HaCSBot.DataBase.Models;

namespace HaCSBot.Services.Services.Extensions
{
    public interface IComplaintAttachmentService
    {
        public Task<ComplaintAttachment> GetByIdAsync(Guid id);
        public Task<IEnumerable<ComplaintAttachment>> GetByTypeAsync(AttachmentType attachmentType);
        public Task<IEnumerable<ComplaintAttachment>> GetByTelegramFileIdAsync(string telegramFileId);
        public Task<IEnumerable<ComplaintAttachment>> GetAllAsync();
        public Task AddAsync(ComplaintAttachment entity);
        public Task UpdateAsync(ComplaintAttachment entity);
        public Task DeleteAsync(Guid id);
    }
}
