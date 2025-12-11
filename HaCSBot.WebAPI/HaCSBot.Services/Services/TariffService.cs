using HaCSBot.DataBase.Repositories.Extensions;
using HaCSBot.Services.Services.Extensions;
using static HaCSBot.Contracts.DTOs.DTOs;

namespace HaCSBot.Services.Services
{
    public class TariffService : ITariffService
    {
        private readonly ITariffRepository _tariffRepository;
        private readonly IMeterReadingRepository _meterReadingRepository;

        public TariffService(ITariffRepository tariffRepository, IMeterReadingRepository meterReadingRepository)
        {
            _tariffRepository = tariffRepository;
            _meterReadingRepository = meterReadingRepository;
        }

        public async Task<List<TariffDto>> GetCurrentTariffsAsync(Guid buildingId)
        {
            var tariffs = await _tariffRepository.GetAllAsync();
            // Фильтр по buildingId, если добавлено в модель
            return tariffs.Select(t => new TariffDto
            {
                Type = t.Type,
                Rate = t.Price 
            }).ToList();
        }

        public async Task<decimal> CalculateCostAsync(Guid apartmentId, TariffDto reading, MeterReadingDto readingDto)
        {
            var tariff = await _tariffRepository.GetByTypeAsync(reading.Type);
            if (tariff == null) throw new InvalidOperationException("Tariff not found");

            var previousReading = await _meterReadingRepository.GetLatestByTypeAsync(apartmentId, readingDto.Type);
            decimal consumption = readingDto.Value - (previousReading?.Value ?? 0);
            return consumption * tariff.Price; // Простой расчет
        }
    }
}