using HaCSBot.Contracts.DTOs;

namespace HaCSBot.Services.Services.Extensions
{
    public interface IBuildingService 
    {
        public Task<BuildingDto?> FindBuildingByAddressAsync(string fullAddress);
        public Task<List<BuildingDto>> GetAdminBuildingsAsync(Guid adminUserId);
        public Task<List<ApartmentDto>> GetApartmentsInBuildingAsync(Guid buildingId);
        public Task<BuildingDto> GetByIdAsync(Guid buildingId);
        public Task<List<BuildingForNotificationDto>> GetAllBuildingsAsync();
    }
}
