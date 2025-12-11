using static HaCSBot.Contracts.DTOs.DTOs;

namespace HaCSBot.Services.Services.Extensions
{
    public interface ITariffService
    {
        Task<List<TariffDto>> GetCurrentTariffsAsync(Guid buildingId);
        Task<decimal> CalculateCostAsync(Guid apartmentId, TariffDto reading, MeterReadingDto readingDto);
    }
}
