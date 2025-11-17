using HaCSBot.DataBase.Enums;
using HaCSBot.DataBase.Models;

namespace HaCSBot.DataBase.Repositories.Extensions
{
    public interface IBuildingRepository : IGenericRepository<Building>
    {
        public Task<Building> GetByAddressAsync(StreetsType streetType, string streetName, string buildingNumber);
    }
}
