using HaCSBot.DataBase.Enums;
using HaCSBot.DataBase.Models;

namespace HaCSBot.DataBase.Repositories.Extensions
{
    public interface ITariffRepository
    {
        public Task<Tariff?> GetByIdAsync(Guid id);
        public Task<Tariff?> GetByTypeAsync(TariffType tariffType);
        public Task<IEnumerable<Tariff>> GetAllAsync();
        public Task AddAsync(Tariff entity);
        public Task UpdateAsync(Tariff entity);
        public Task DeleteAsync(Guid id);
    }
}
