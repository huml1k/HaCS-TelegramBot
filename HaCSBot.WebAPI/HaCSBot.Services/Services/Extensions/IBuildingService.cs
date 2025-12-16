using HaCSBot.Contracts.DTOs;
using HaCSBot.DataBase.Enums;
using HaCSBot.DataBase.Models;

namespace HaCSBot.Services.Services.Extensions
{
    public interface IBuildingService 
    {
        public Task<BuildingDto?> FindBuildingByAddressAsync(string fullAddress);
        public Task<List<BuildingDto>> GetAdminBuildingsAsync(Guid adminUserId);
        public Task<List<ApartmentDto>> GetApartmentsInBuildingAsync(Guid buildingId);
        public Task<BuildingDto> GetByIdAsync(Guid buildingId);


	}
}
