using HaCSBot.DataBase.Models;
using HaCSBot.DataBase.Repositories.Extensions;
using Microsoft.EntityFrameworkCore;

namespace HaCSBot.DataBase.Repositories
{
    public class ApartmentRepository : IApartmentRepository
    {
        private readonly MyApplicationDbContext _context;

        public ApartmentRepository(MyApplicationDbContext context) 
        {
            _context = context;
        }

        public async Task Create(Apartment entity)
        {
            await _context.Apartments.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(Guid id)
        {
            var entity = await GetById(id);
            if (entity != null)
            {
                _context.Apartments.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Apartment>> GetAll()
        {
            return await _context.Apartments.ToListAsync();
        }

        public async Task<IEnumerable<Apartment>> GetByBuildingIdAsync(Guid buildingId)
        {
            return await _context.Apartments.Where(a => a.BuildingId == buildingId).ToListAsync();
        }

        public async Task<Apartment> GetById(Guid id)
        {
            return await _context.Apartments.FindAsync(id);
        }

        public async Task<Apartment> GetByUserIdAsync(Guid userId)
        {
            return await _context.Apartments.FirstOrDefaultAsync(a => a.UserId == userId);
        }

        public async Task Update(Apartment entity)
        {
            _context.Apartments.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}
