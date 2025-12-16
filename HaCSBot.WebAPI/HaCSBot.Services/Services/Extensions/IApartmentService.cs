using HaCSBot.Contracts.DTOs;
using HaCSBot.DataBase.Models;

namespace HaCSBot.Services.Services.Extensions
{
    public interface IApartmentService
    {
        public Task<List<ApartmentDto>> GetByUserIdAsync(Guid userId);
    }
}
