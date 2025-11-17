using HaCSBot.DataBase.Enums;
using HaCSBot.DataBase.Models;
using HaCSBot.DataBase.Repositories.Extensions;
using Microsoft.EntityFrameworkCore;

namespace HaCSBot.DataBase.Repositories
{
    public class BuildingMaintenanceRepository : IBuildingMaintenanceRepository
    {
        private readonly MyApplicationDbContext _context;

        public BuildingMaintenanceRepository(MyApplicationDbContext context) 
        {
            _context = context;
        }

        public async Task Create(BuildingMaintenance entity)
        {
            await _context.BuildingMaintenances.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(Guid id)
        {
            var entity = await GetById(id);
            if (entity != null)
            {
                _context.BuildingMaintenances.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<BuildingMaintenance>> GetAll()
        {
            return await _context.BuildingMaintenances.ToListAsync();
        }

        public async Task<IEnumerable<BuildingMaintenance>> GetByBuildingIdAsync(Guid buildingId)
        {
            return await _context.BuildingMaintenances
                .Where(m => m.BuildingId == buildingId)
                .ToListAsync();
        }

        public async Task<BuildingMaintenance> GetById(Guid id)
        {
            return await _context.BuildingMaintenances.FindAsync(id);
        }

        public async Task<IEnumerable<BuildingMaintenance>> GetByStatusAsync(StatusMaintenance status)
        {
            return await _context.BuildingMaintenances
                .Where(x => x.Status == status)
                .ToListAsync();
        }

        public async Task Update(BuildingMaintenance entity)
        {
            _context.BuildingMaintenances.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}
