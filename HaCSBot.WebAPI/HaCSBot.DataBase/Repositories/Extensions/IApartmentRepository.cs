using HaCSBot.DataBase.Models;

namespace HaCSBot.DataBase.Repositories.Extensions
{
    public interface IApartmentRepository : IGenericRepository<Apartment>
    {
        public Task<IEnumerable<Apartment>> GetByBuildingIdAsync(Guid buildingId);
        public Task<Apartment> GetByUserIdAsync(Guid userId);
    }
}
