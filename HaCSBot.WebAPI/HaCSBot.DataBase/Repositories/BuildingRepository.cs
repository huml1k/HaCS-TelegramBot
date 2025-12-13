using HaCSBot.DataBase.Enums;
using HaCSBot.DataBase.Models;
using HaCSBot.DataBase.Repositories.Extensions;
using Microsoft.EntityFrameworkCore;

namespace HaCSBot.DataBase.Repositories
{
    public class BuildingRepository : IBuildingRepository
    {
        private readonly MyApplicationDbContext _context;

        public BuildingRepository(MyApplicationDbContext context)
        {
            _context = context;
        }

		public async Task AddAsync(Building building)
		{
			await _context.Buildings.AddAsync(building);
			await _context.SaveChangesAsync();
		}

		public async Task DeleteAsync(Guid id)
		{
			var entity = await _context.Buildings.FindAsync(id);
			if (entity != null)
			{
				_context.Buildings.Remove(entity);
				await _context.SaveChangesAsync();
			}
		}

		public async Task<bool> ExistsAsync(Guid id)
		{
			return await (_context.Buildings.FirstOrDefaultAsync(b => b.Id == id)) != null;
		}

		public async Task<List<Building>> GetAllAsync()
		{
			return await _context.Buildings
				.AsNoTracking()
				.OrderBy(b => b.StreetName)
				.ThenBy(b => b.BuildingNumber)
				.ToListAsync();
		}

		public async Task<List<Building>> GetAllWithApartmentsAsync()
		{
			return await _context.Buildings
				.AsNoTracking()
				.Include(b => b.Apartments)
				.ThenInclude(a => a.User)
				.OrderBy(b => b.StreetName)
				.ThenBy(b => b.BuildingNumber)
				.ToListAsync();
		}

		public async Task<List<Building>> GetBuildingsByUserIdAsync(Guid userId)
		{
			return await _context.Buildings
				.AsNoTracking()
				.Include(b => b.Apartments)
				.ThenInclude(a => a.User)
				.Where(b => b.Apartments.Any(a => a.UserId == userId))
				.OrderBy(b => b.StreetName)
				.ThenBy(b => b.BuildingNumber)
				.ToListAsync();
		}

		public async Task<Building?> GetByFullAddressAsync(StreetsType streetType, string streetName, string buildingNumber)
		{
            var normalizedStreet = streetName.Trim();  
            var normalizedNumber = buildingNumber.Trim();

            return await _context.Buildings
                .AsNoTracking()
                .Include(b => b.Apartments).ThenInclude(a => a.User)
                .FirstOrDefaultAsync(b =>
                    b.StreetType == streetType &&
                    b.StreetName == normalizedStreet &&  
                    b.BuildingNumber.Trim() == normalizedNumber);
        }

		public async Task<Building?> GetByIdAsync(Guid id)
		{
			return await _context.Buildings.FirstOrDefaultAsync(b => b.Id == id);
		}

		public async Task UpdateAsync(Building building)
		{
			_context.Buildings.Update(building);
			await _context.SaveChangesAsync();
		}
	}

}
