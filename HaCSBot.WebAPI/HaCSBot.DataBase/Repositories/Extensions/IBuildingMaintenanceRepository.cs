using HaCSBot.DataBase.Enums;
using HaCSBot.DataBase.Models;

namespace HaCSBot.DataBase.Repositories.Extensions
{
    public interface IBuildingMaintenanceRepository : IGenericRepository<BuildingMaintenance>
    {
        public Task<IEnumerable<BuildingMaintenance>> GetByBuildingIdAsync(Guid buildingId);
        public Task<IEnumerable<BuildingMaintenance>> GetByStatusAsync(StatusMaintenance status);
    }
}
