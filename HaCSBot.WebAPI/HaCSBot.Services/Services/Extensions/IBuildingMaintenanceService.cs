using HaCSBot.DataBase.Enums;
using HaCSBot.DataBase.Models;

namespace HaCSBot.Services.Services.Extensions
{
    public interface IBuildingMaintenanceService 
    {
        public Task<IEnumerable<BuildingMaintenance>> GetByBuildingIdAsync(Guid buildingId);
        public Task<IEnumerable<BuildingMaintenance>> GetByStatusAsync(StatusMaintenance status);
    }
}
