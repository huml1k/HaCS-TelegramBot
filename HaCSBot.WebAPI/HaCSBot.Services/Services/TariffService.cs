using AutoMapper;
using HaCSBot.Contracts.DTOs;
using HaCSBot.DataBase.Models;
using HaCSBot.DataBase.Repositories.Extensions;
using HaCSBot.Services.Services.Extensions;

namespace HaCSBot.Services.Services
{
    public class TariffService : ITariffService
    {
        private readonly ITariffRepository _tariffRepository;
        private readonly IMeterReadingRepository _meterReadingRepository;
        private readonly IMapper _mapper;

		public TariffService(ITariffRepository tariffRepository,
            IMeterReadingRepository meterReadingRepository,
            IMapper mapper)
        {
            _tariffRepository = tariffRepository;
            _meterReadingRepository = meterReadingRepository;
            _mapper = mapper;

		}

		public async Task<List<TariffDto>> GetCurrentTariffsAsync(Guid buildingId)
		{
			var tariffs = await _tariffRepository.GetByBuildingIdAsync(buildingId);

            return _mapper.Map<List<TariffDto>>(tariffs);

		}

		public async Task<decimal> CalculateCostAsync(Guid apartmentId, TariffDto reading, MeterReadingDto readingDto)
        {
            var tariff = await _tariffRepository.GetByTypeAsync(reading.Type);
            if (tariff == null) throw new InvalidOperationException("Tariff not found");

            var previousReading = await _meterReadingRepository.GetLatestByTypeAsync(apartmentId, readingDto.Type);
            decimal consumption = readingDto.Value - (previousReading?.Value ?? 0);
            return consumption * tariff.Price; 
        }
    }
}