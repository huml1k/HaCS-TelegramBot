using HaCSBot.DataBase.Repositories.Extensions;
using HaCSBot.Services.Services.Extensions;

namespace HaCSBot.Services.Services
{
    public class NotificationAttachmentService : INotificationAttachmentService
    {
        private readonly INotificationAttachmentRepository _repository;

        public NotificationAttachmentService(INotificationAttachmentRepository repository)
        {
            _repository = repository;
        }
    }
}
