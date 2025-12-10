using HaCSBot.DataBase.Enums;
using HaCSBot.DataBase.Models;
using HaCSBot.DataBase.Repositories.Extensions;
using Microsoft.EntityFrameworkCore;

namespace HaCSBot.DataBase.Repositories
{
    public class MeterReadingRepository : IMeterReadingRepository   
    {
        private readonly MyApplicationDbContext _context;
        public MeterReadingRepository(MyApplicationDbContext context)
        { 
            _context = context;
        }

        public async Task AddAsync(MeterReading entity)
        {
            await _context.MeterReading.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var result = await GetByIdAsync(id);
            if (result != null)
            {
                _context.MeterReading.Remove(result);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<MeterReading>> GetAllAsync()
        {
            return await _context.MeterReading
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<MeterReading?> GetByIdAsync(Guid id)
        {
            return await _context.MeterReading
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<MeterReading>> GetMeterReadingByType(MeterType meterType)
        {
            return await _context.MeterReading
                .AsNoTracking()
                .Where(x => x.Type == meterType)
                .ToListAsync();
        }

        public async Task<IEnumerable<MeterReading>> GetSubmittedByUserId(Guid id)
        {
            return await _context.MeterReading
                .AsNoTracking()
                .Where(x => x.SubmittedByUserId == id)
                .ToListAsync();
        }

        public async Task UpdateAsync(MeterReading entity)
        {
            _context.MeterReading.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}
