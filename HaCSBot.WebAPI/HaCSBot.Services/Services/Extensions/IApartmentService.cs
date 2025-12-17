using HaCSBot.Contracts.DTOs;
using HaCSBot.DataBase.Models;

namespace HaCSBot.Services.Services.Extensions
{
    public interface IApartmentService
    {
        Task<List<ApartmentDto>> GetByUserIdAsync(Guid userId);
        Task<ApartmentDto?> GetByIdAsync(Guid apartmentId);
        Task<List<ApartmentDto>> GetApartmentsInBuildingAsync(Guid buildingId);
    }
}
