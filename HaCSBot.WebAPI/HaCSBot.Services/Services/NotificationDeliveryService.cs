using HaCSBot.DataBase.Repositories.Extensions;
using HaCSBot.Services.Services.Extensions;

namespace HaCSBot.Services.Services
{
    public class NotificationDeliveryService : INotificationDeliveryService
    {
        private readonly INotificationDeliveryRepository _repository;

        public NotificationDeliveryService(INotificationDeliveryRepository repository)
        {
            _repository = repository;
        }
    }
}
