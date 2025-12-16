using HaCSBot.Contracts.DTOs;

namespace HaCSBot.Services.Services.Extensions
{
    public interface ITariffService
    {
        Task<List<TariffDto>> GetCurrentTariffsAsync(Guid buildingId);
        Task<decimal> CalculateCostAsync(Guid apartmentId, TariffDto reading, MeterReadingDto readingDto);
    }
}
