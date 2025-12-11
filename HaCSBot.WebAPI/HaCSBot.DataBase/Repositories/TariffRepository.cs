using HaCSBot.DataBase.Enums;
using HaCSBot.DataBase.Models;
using HaCSBot.DataBase.Repositories.Extensions;
using Microsoft.EntityFrameworkCore;

namespace HaCSBot.DataBase.Repositories
{
    public class TariffRepository : ITariffRepository
    {
        private readonly MyApplicationDbContext _context;

        public TariffRepository(MyApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Tariff entity)
        {
            await _context.Tariffs.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var result = await GetByIdAsync(id);
            if (result != null)
            {
                _context.Tariffs.Remove(result);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Tariff>> GetAllAsync()
        {
            return await _context.Tariffs
                 .AsNoTracking()
                 .ToListAsync();
        }

        public async Task<Tariff?> GetByIdAsync(Guid id)
        {
            return await _context.Tariffs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Tariff?> GetByTypeAsync(TariffType tariffType)
        {
            return await _context.Tariffs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Type == tariffType);
        }

        public async Task UpdateAsync(Tariff entity)
        {
            _context.Tariffs.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}