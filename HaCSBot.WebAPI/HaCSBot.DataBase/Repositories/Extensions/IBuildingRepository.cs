using HaCSBot.DataBase.Enums;
using HaCSBot.DataBase.Models;

namespace HaCSBot.DataBase.Repositories.Extensions
{
    public interface IBuildingRepository 
    {
		public Task<Building> GetByIdAsync(Guid id);
		public Task<Building> GetByFullAddressAsync(StreetsType streetType, string streetName, string buildingNumber);
		public Task<List<Building>> GetAllAsync();
		public Task<List<Building>> GetAllWithApartmentsAsync();
		public Task AddAsync(Building building);
		public Task UpdateAsync(Building building);
		public Task DeleteAsync(Guid id);
		public Task<bool> ExistsAsync(Guid id);
		public Task<List<Building>> GetBuildingsByUserIdAsync(Guid userId); 

	}
}
