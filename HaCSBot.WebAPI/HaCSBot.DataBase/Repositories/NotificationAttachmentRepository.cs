using HaCSBot.DataBase.Enums;
using HaCSBot.DataBase.Models;
using HaCSBot.DataBase.Repositories.Extensions;
using Microsoft.EntityFrameworkCore;

namespace HaCSBot.DataBase.Repositories
{
    public class NotificationAttachmentRepository : INotificationAttachmentRepository
    {
        private readonly MyApplicationDbContext _context;

        public NotificationAttachmentRepository(MyApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(NotificationAttachment entity)
        {
            await _context.NotificationAttachments.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var result = await GetByIdAsync(id);
            if (result != null)
            {
                _context.NotificationAttachments.Remove(result);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<NotificationAttachment>> GetAllAsync()
        {
            return await _context.NotificationAttachments
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<NotificationAttachment?> GetByIdAsync(Guid id)
        {
            return await _context.NotificationAttachments
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<NotificationAttachment>> GetByTelegramFileIdAsync(string telegramFileId)
        {
            return await _context.NotificationAttachments
                .AsNoTracking()
                .Where(x => x.TelegramFileId == telegramFileId)
                .ToListAsync();
        }

        public async Task<IEnumerable<NotificationAttachment>> GetByTypeAsync(AttachmentType attachmentType)
        {
            return await _context.NotificationAttachments
                .AsNoTracking()
                .Where(x => x.Type == attachmentType)
                .ToListAsync();
        }

        public async Task UpdateAsync(NotificationAttachment entity)
        {
            _context.NotificationAttachments.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}
