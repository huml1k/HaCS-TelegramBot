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

		public async Task AddAsync(Apartment apartment)
		{
			await _context.Apartments.AddAsync(apartment);
			await _context.SaveChangesAsync();
		}

		public async Task DeleteAsync(Guid id)
		{
			var entity = await GetByIdAsync(id);
			if (entity != null)
			{
				_context.Apartments.Remove(entity);
				await _context.SaveChangesAsync();
			}
		}

		public async Task<bool> ExistsInBuildingAsync(string apartmentNumber, Guid buildingId)
		{
			return await _context.Apartments
				.AnyAsync(a => a.ApartmentNumber == apartmentNumber && a.BuildingId == buildingId);
		}

		public async Task<IEnumerable<Apartment>> GetAll()
        {
            return await _context.Apartments.ToListAsync();
        }

		public async Task<List<Apartment>> GetApartmentsByBuildingIdAsync(Guid buildingId)
		{
			return await _context.Apartments
				.AsNoTracking()
				.Include(b => b.Building)
				.Where(Building => buildingId == Building.Id)
				.ToListAsync();
		}

		public async Task<IEnumerable<Apartment>> GetByBuildingIdAsync(Guid buildingId)
        {
            return await _context.Apartments.Where(a => a.BuildingId == buildingId).ToListAsync();
        }

        public async Task<Apartment?> GetByIdAsync(Guid id)
        {
            return await _context.Apartments
				.AsNoTracking()
				.Include(a => a.Building)
				.FirstOrDefaultAsync(a => a.Id == id);
        }

		public async Task<Apartment?> GetByNumberAndBuildingIdAsync(string apartmentNumber, Guid buildingId)
		{
			return await _context.Apartments
				.FirstOrDefaultAsync(a => a.ApartmentNumber == apartmentNumber && a.BuildingId == buildingId);
		}

		public async Task<List<Apartment>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Apartments
				.AsNoTracking()
				.Include(a => a.Building)  
				.Include(a => a.User)
				.Where(a => a.UserId == userId)
				.ToListAsync();
		}

		public async Task<List<Apartment>> GetUnoccupiedApartmentsAsync(Guid buildingId)
		{
			return await _context.Apartments
				.AsNoTracking()
				.Include(a => a.Building)
				.Include(a => a.User)
				.Where(a => a.BuildingId == buildingId && a.UserId == null)
				.ToListAsync();
		}

		public async Task<Apartment?> GetWithUserAndBuildingAsync(Guid apartmentId)
		{
			return await _context.Apartments
				.AsNoTracking()
				.Include(a => a.User)
				.Include(a => a.Building)
				.FirstOrDefaultAsync(a => a.Id == apartmentId);
		}

		public async Task UpdateAsync(Apartment apartment)
		{
            _context.Apartments.Update(apartment);
            await _context.SaveChangesAsync();
        }
	}
}
