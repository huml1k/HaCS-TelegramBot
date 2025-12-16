using HaCSBot.Contracts.DTOs;

namespace HaCSBot.Services.Services.Extensions
{
    public interface IMeterReadingService
    {
        public Task SubmitMeterReadingAsync(SubmitMeterReadingDto dto, long telegramId);
        public Task<List<MeterReadingDto>> GetLastReadingsAsync(Guid apartmentId);
        public Task<List<MeterReadingDto>> GetHistoryAsync(Guid apartmentId, int months = 12);
        public Task<ConsumptionDto> GetCurrentConsumptionAsync(Guid apartmentId);
    }
}
