using HaCSBot.DataBase.Enums;
using HaCSBot.DataBase.Models;
using HaCSBot.DataBase.Repositories.Extensions;
using Microsoft.EntityFrameworkCore;

namespace HaCSBot.DataBase.Repositories
{
    public class ComplaintAttachmentRepository : IComplaintAttachmentRepository
    {
        private readonly MyApplicationDbContext _context;
        public ComplaintAttachmentRepository( MyApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(ComplaintAttachment entity)
        {
            await _context.ComplaintAttachments.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var result = await GetByIdAsync(id);
            if (result != null)
            {
                _context.ComplaintAttachments.Remove(result);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<ComplaintAttachment>> GetAllAsync()
        {
            return await _context.ComplaintAttachments
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<ComplaintAttachment?> GetByIdAsync(Guid id)
        {
            return await _context.ComplaintAttachments
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<ComplaintAttachment>> GetByTelegramFileIdAsync(string telegramFileId)
        {
            return await _context.ComplaintAttachments
                .AsNoTracking()
                .Where(c => c.TelegramFileId == telegramFileId)
                .ToListAsync();
        }

        public async Task<IEnumerable<ComplaintAttachment>> GetByTypeAsync(AttachmentType attachmentType)
        {
            return await _context.ComplaintAttachments
                .AsNoTracking()
                .Where(c => c.Type == attachmentType)
                .ToListAsync();
        }

        public async Task UpdateAsync(ComplaintAttachment entity)
        {
            _context.ComplaintAttachments.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}
