using AutoMapper;
using HaCSBot.Contracts.DTOs;
using HaCSBot.DataBase.Enums;
using HaCSBot.DataBase.Models;
using HaCSBot.DataBase.Repositories.Extensions;
using HaCSBot.Services.Services.Extensions;

namespace HaCSBot.Services.Services
{
    public class BuildingService : IBuildingService
    {
        private readonly IBuildingRepository _buildingRepository;
        private readonly IApartmentRepository _apartmentRepository;
        private readonly IMapper _mapper;

        public BuildingService(
            IBuildingRepository buildingRepository,
            IApartmentRepository apartmentRepository,
			IMapper mapper)
        {
            _buildingRepository = buildingRepository;
            _apartmentRepository = apartmentRepository;
            _mapper = mapper;
        }

		public async Task<BuildingDto?> FindBuildingByAddressAsync(string fullAddress)
		{
			if (string.IsNullOrWhiteSpace(fullAddress))
				return null;

			var (streetType, streetName, buildingNumber) = ParseAddress(fullAddress);

			if (streetType == null || string.IsNullOrWhiteSpace(streetName) || string.IsNullOrWhiteSpace(buildingNumber))
				return null;

			var building = await _buildingRepository.GetByFullAddressAsync(streetType.Value, streetName, buildingNumber);

			if (building == null)
				return null;

			return _mapper.Map<BuildingDto>(building);
		}

		public async Task<List<BuildingDto>> GetAdminBuildingsAsync(Guid adminUserId)
		{
			var buildings = await _buildingRepository.GetBuildingsByUserIdAsync(adminUserId);
			return _mapper.Map<List<BuildingDto>>(buildings);
		}

		public async Task<BuildingDto> GetByIdAsync(Guid buildingId)
		{
			var buildings = await _buildingRepository.GetByIdAsync(buildingId);
			return _mapper.Map<BuildingDto>(buildings);
		}

		public async Task<List<ApartmentDto>> GetApartmentsInBuildingAsync(Guid buildingId)
		{
			var apartments = await _apartmentRepository.GetApartmentsByBuildingIdAsync(buildingId);
			return _mapper.Map<List<ApartmentDto>>(apartments);
		}

        public async Task<List<BuildingForNotificationDto>> GetAllBuildingsAsync()
        {
            var buildings = await _buildingRepository.GetAllAsync();
            return _mapper.Map<List<BuildingForNotificationDto>>(buildings);
        }


        private (StreetsType? streetType, string streetName, string buildingNumber) ParseAddress(string fullAddress)
		{
			var normalized = fullAddress.Trim().ToLowerInvariant();

			// Словарь для распознавания типа улицы
			var streetTypes = new Dictionary<string, StreetsType>
			{
				{ "улица", StreetsType.Улица },
				{ "проспект", StreetsType.Проспект },
				{ "переулок", StreetsType.Переулок },
				{ "бульвар", StreetsType.Бульвар },
				{ "площадь", StreetsType.Площадь },
				{ "проезд", StreetsType.Проезд },
				{ "шоссе", StreetsType.Шоссе }
			};

			StreetsType? foundType = null;
			string foundTypeStr = string.Empty;

			//тип улицы
			foreach (var type in streetTypes.Keys)
			{
				if (normalized.Contains(type))
				{
					foundType = streetTypes[type];
					foundTypeStr = type;
					break;
				}
			}

			if (foundType == null)
				return (null, "", "");

			var withoutType = normalized.Replace(foundTypeStr, "").Trim();

			var parts = withoutType.Split(' ', StringSplitOptions.RemoveEmptyEntries);

			//номер дома
			if (parts.Length < 2)
				return (null, "", "");

			var buildingNum = parts[^1];
			var streetName = string.Join(" ", parts[0..^1]);

			if (string.IsNullOrWhiteSpace(streetName) || string.IsNullOrWhiteSpace(buildingNum))
				return (null, "", "");

			streetName = char.ToUpperInvariant(streetName[0]) + streetName.Substring(1);

			return (foundType, streetName, buildingNum);
		}

	}

}
