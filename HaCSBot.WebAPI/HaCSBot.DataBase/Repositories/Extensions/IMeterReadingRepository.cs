using HaCSBot.DataBase.Enums;
using HaCSBot.DataBase.Models;

namespace HaCSBot.DataBase.Repositories.Extensions
{
    public interface IMeterReadingRepository
    {
        public Task<MeterReading> GetByIdAsync(Guid id);
        public Task<IEnumerable<MeterReading>> GetMeterReadingByType(MeterType meterType);
        public Task<IEnumerable<MeterReading>> GetSubmittedByUserId(Guid id);
        public Task<IEnumerable<MeterReading>> GetAllAsync();
        public Task AddAsync(MeterReading entity);
        public Task UpdateAsync(MeterReading entity);
        public Task DeleteAsync(Guid id);
    }
}
