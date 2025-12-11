using static HaCSBot.Contracts.DTOs.DTOs;

namespace HaCSBot.Services.Services.Extensions
{
    public interface IMeterReadingService
    {
        public Task SubmitMeterReadingAsync(SubmitReadingDto dto, long telegramId);
        public Task<List<MeterReadingDto>> GetLastReadingsAsync(Guid apartmentId);
        public Task<List<MeterReadingHistoryDto>> GetHistoryAsync(Guid apartmentId, int months = 12);
        public Task<ConsumptionDto> GetCurrentConsumptionAsync(Guid apartmentId);
    }
}
