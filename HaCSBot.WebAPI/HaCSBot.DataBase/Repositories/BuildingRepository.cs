using HaCSBot.DataBase.Enums;
using HaCSBot.DataBase.Models;
using HaCSBot.DataBase.Repositories.Extensions;
using Microsoft.EntityFrameworkCore;

namespace HaCSBot.DataBase.Repositories
{
    /// <summary>
    /// Написать логику CRUD операция для работы домов. 
    /// Для реализации можно использовать интерфейc IGenericRepository
    /// или создать свой интерфейс с расширенным фунционалом
    /// </summary>
    public class BuildingRepository : IBuildingRepository
    {
        private readonly MyApplicationDbContext _context;

        public BuildingRepository(MyApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Create(Building entity)
        {
            await _context.Buildings.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(Guid id)
        {
            var entity = await GetById(id);
            if (entity != null)
            {
                _context.Buildings.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Building>> GetAll()
        {
            return await _context.Buildings.ToListAsync();
        }

        public async Task<Building> GetByAddressAsync(StreetsType streetType, string streetName, string buildingNumber)
        {
            return await _context.Buildings.FirstOrDefaultAsync(b => b.StreetType == streetType &&
                                                         b.StreetName == streetName &&
                                                         b.BuildingNumber == buildingNumber);
        }

        public async Task<Building> GetById(Guid id)
        {
            return await _context.Buildings.FindAsync(id);
        }

        public async Task Update(Building entity)
        {
            _context.Buildings.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}
