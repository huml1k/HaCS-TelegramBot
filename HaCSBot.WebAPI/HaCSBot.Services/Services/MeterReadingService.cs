using HaCSBot.DataBase.Models;
using HaCSBot.DataBase.Repositories.Extensions;
using HaCSBot.Services.Services.Extensions;
using static HaCSBot.Contracts.DTOs.DTOs;

namespace HaCSBot.Services.Services
{
    public class MeterReadingService : IMeterReadingService
    {
        private readonly IMeterReadingRepository _meterReadingRepository;
        private readonly IApartmentRepository _apartmentRepository;
        private readonly IUserRepository _userRepository;

        public MeterReadingService(IMeterReadingRepository meterReadingRepository,
            IApartmentRepository apartmentRepository,
            IUserRepository userRepository)
        {
            _meterReadingRepository = meterReadingRepository;
            _apartmentRepository = apartmentRepository;
            _userRepository = userRepository;
        }

        public async Task SubmitMeterReadingAsync(SubmitReadingDto dto, long telegramId)
        {
            var user = await _userRepository.GetByTelegramIdAsync(telegramId);
            if (user == null) throw new InvalidOperationException("User not found");

            var apartment = await _apartmentRepository.GetByIdAsync(dto.ApartmentId);
            if (apartment == null || apartment.UserId != user.Id) throw new InvalidOperationException("Apartment not found or not owned");

            var reading = new MeterReading
            {
                ApartmentId = dto.ApartmentId,
                Type = dto.Type,
                Value = dto.Value,
                ReadingDate = DateTime.UtcNow,
                SubmittedByUserId = user.Id
            };
            await _meterReadingRepository.AddAsync(reading);
        }

        public async Task<List<MeterReadingDto>> GetLastReadingsAsync(Guid apartmentId)
        {
            var readings = await _meterReadingRepository.GetByApartmentIdAsync(apartmentId);
            return readings.GroupBy(r => r.Type)
                .Select(g => g.OrderByDescending(r => r.ReadingDate).First())
                .Select(r => new MeterReadingDto { Type = r.Type, Value = r.Value, Date = r.ReadingDate }).ToList();
        }

        public async Task<List<MeterReadingHistoryDto>> GetHistoryAsync(Guid apartmentId, int months = 12)
        {
            var fromDate = DateTime.UtcNow.AddMonths(-months);
            var readings = await _meterReadingRepository.GetByApartmentIdAsync(apartmentId);
            readings = readings.Where(r => r.ReadingDate >= fromDate).ToList();
            return readings.Select(r => new MeterReadingHistoryDto
            {
                Type = r.Type,
                Value = r.Value,
                Date = r.ReadingDate
            }).ToList();
        }

        public async Task<ConsumptionDto> GetCurrentConsumptionAsync(Guid apartmentId)
        {
            var readings = await GetHistoryAsync(apartmentId, 1); // За месяц
            // Расчет расхода (пример)
            return new ConsumptionDto { Water = 5, Electricity = 120 }; // Реализуйте логику
        }
    }
}
