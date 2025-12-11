using HaCSBot.DataBase.Enums;
using HaCSBot.DataBase.Models;
using static HaCSBot.Contracts.DTOs.DTOs;

namespace HaCSBot.Services.Services.Extensions
{
    public interface IBuildingService 
    {
        //public Task<BuildingDto?> FindBuildingByAddressAsync(string fullAddress);
        public Task<List<BuildingDto>> GetAdminBuildingsAsync(Guid adminUserId);
        public Task<List<ApartmentDto>> GetApartmentsInBuildingAsync(Guid buildingId);
        public Task<List<ApartmentDto>> GetFreeApartmentsAsync(Guid buildingId);

    }
}
