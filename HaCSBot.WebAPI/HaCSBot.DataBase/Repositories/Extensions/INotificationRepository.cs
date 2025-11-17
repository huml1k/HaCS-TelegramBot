using HaCSBot.DataBase.Enums;
using HaCSBot.DataBase.Models;

namespace HaCSBot.DataBase.Repositories.Extensions
{
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        public Task<IEnumerable<Notification>> GetByBuildingIdAsync(Guid buildingId);
        public Task<IEnumerable<Notification>> GetByTypeAsync(NotificationType type);
    }
}
