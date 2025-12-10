using HaCSBot.DataBase.Enums;
using HaCSBot.DataBase.Models;
using HaCSBot.DataBase.Repositories.Extensions;
using HaCSBot.Services.Services.Extensions;

namespace HaCSBot.Services.Services
{
    public class BuildingService : IBuildingService
    {
        private readonly IBuildingRepository _repository;

        public BuildingService(IBuildingRepository repository)
        {
            _repository = repository;
        }

        public Task AddAsync(Building building)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistsAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<List<Building>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<List<Building>> GetAllWithApartmentsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<List<Building>> GetBuildingsByUserIdAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<Building> GetByFullAddressAsync(StreetsType streetType, string streetName, string buildingNumber)
        {
            throw new NotImplementedException();
        }

        public Task<Building> GetByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Building building)
        {
            throw new NotImplementedException();
        }
    }

}
