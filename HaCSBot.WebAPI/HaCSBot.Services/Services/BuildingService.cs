using HaCSBot.DataBase.Enums;
using HaCSBot.DataBase.Models;
using HaCSBot.DataBase.Repositories.Extensions;
using HaCSBot.Services.Services.Extensions;
using static HaCSBot.Contracts.DTOs.DTOs;

namespace HaCSBot.Services.Services
{
    public class BuildingService : IBuildingService
    {
        private readonly IBuildingRepository _buildingRepository;
        private readonly IApartmentRepository _apartmentRepository;

        public BuildingService(IBuildingRepository buildingRepository, IApartmentRepository apartmentRepository)
        {
            _buildingRepository = buildingRepository;
            _apartmentRepository = apartmentRepository;
        }

        //public async Task<BuildingDto?> FindBuildingByAddressAsync(string fullAddress)
        //{
        //    // Парсинг fullAddress на StreetType, StreetName, BuildingNumber (реализуйте парсер)
        //    // Пример: assume parsed
        //    //var streetType = StreetsType.; // Парсинг
        //    //var streetName = "Ленина";
        //    //var buildingNumber = "25";
        //    //var building = await _buildingRepository.GetByFullAddressAsync(streetType, streetName, buildingNumber);
        //    //if (building == null) return null;

        //    //return new BuildingDto
        //    //{
        //    //    Id = building.Id,
        //    //    FullAddress = $"{building.StreetType} {building.StreetName}, {building.BuildingNumber}"
        //    //};
        //}

        public async Task<List<BuildingDto>> GetAdminBuildingsAsync(Guid adminUserId)
        {
            var buildings = await _buildingRepository.GetBuildingsByUserIdAsync(adminUserId);
            return buildings.Select(b => new BuildingDto
            {
                Id = b.Id,
                FullAddress = $"{b.StreetType} {b.StreetName}, {b.BuildingNumber}"
            }).ToList();
        }

        public async Task<List<ApartmentDto>> GetApartmentsInBuildingAsync(Guid buildingId)
        {
            var apartments = await _apartmentRepository.GetApartmentsByBuildingIdAsync(buildingId);
            return apartments.Select(a => new ApartmentDto
            {
                Id = a.Id,
                Number = a.ApartmentNumber,
                OwnerName = a.User != null ? $"{a.User.FirstName} {a.User.LastName}" : "Свободна"
            }).ToList();
        }

        public async Task<List<ApartmentDto>> GetFreeApartmentsAsync(Guid buildingId)
        {
            var apartments = await _apartmentRepository.GetUnoccupiedApartmentsAsync(buildingId);
            return apartments.Select(a => new ApartmentDto
            {
                Id = a.Id,
                Number = a.ApartmentNumber
            }).ToList();
        }
    }

}
