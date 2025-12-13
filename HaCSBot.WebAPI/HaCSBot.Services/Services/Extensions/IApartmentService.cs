using HaCSBot.DataBase.Models;

namespace HaCSBot.Services.Services.Extensions
{
    public interface IApartmentService
    {
        public Task<List<Apartment>> GetByUserIdAsync(Guid userId);
    }
}
