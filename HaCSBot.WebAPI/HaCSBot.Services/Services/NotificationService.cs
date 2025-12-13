using HaCSBot.DataBase.Models;
using HaCSBot.DataBase.Repositories.Extensions;
using HaCSBot.Services.Services.Extensions;
using static HaCSBot.Contracts.DTOs.DTOs;

namespace HaCSBot.Services.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IBuildingRepository _buildingRepository;
        private readonly IApartmentRepository _apartmentRepository;
        private readonly IUserRepository _userRepository;
        private readonly INotificationDeliveryRepository _deliveryRepository;

        public NotificationService(INotificationRepository notificationRepository,
            IBuildingRepository buildingRepository,
            IApartmentRepository apartmentRepository,
            IUserRepository userRepository,
            INotificationDeliveryRepository deliveryRepository)
        {
            _notificationRepository = notificationRepository;
            _buildingRepository = buildingRepository;
            _apartmentRepository = apartmentRepository;
            _userRepository = userRepository;
            _deliveryRepository = deliveryRepository;
        }

        public async Task<Guid> CreateNotificationAsync(CreateNotificationDto dto, Guid adminId)
        {
            var notification = new Notification
            {
                Type = dto.Type,
                Message = dto.Message,
                BuildingId = dto.BuildingId,
                BuildingMaintenanceId = dto.BuildingMaintenanceId,
                CreatedByUserId = adminId,
                CreatedDate = DateTime.UtcNow,
                ScheduledSendDate = dto.ScheduledSendDate,
                Attachments = dto.Attachments.Select(a => new NotificationAttachment { Type = a.Type, TelegramFileId = a.TelegramFileId, Caption = a.Caption }).ToList()
            };
            await _notificationRepository.AddAsync(notification);
            return notification.Id;
        }

        public async Task SendNotificationToBuildingAsync(Guid notificationId)
        {
            var notification = await _notificationRepository.GetByIdWithDetailsAsync(notificationId);
            if (notification == null || notification.BuildingId == null) throw new InvalidOperationException("Notification or building not found");

            var apartments = await _apartmentRepository.GetApartmentsByBuildingIdAsync(notification.BuildingId.Value);
            foreach (var apartment in apartments)
            {
                if (apartment.UserId != null)
                {
                    var user = await _userRepository.GetByIdAsync(apartment.UserId);
                    if (user?.TelegramId != null)
                    {
                        var delivery = new NotificationDelivery
                        {
                            NotificationId = notificationId,
                            TelegramUserId = user.TelegramId.Value,
                            DeliveredDate = DateTime.UtcNow,
                            SentDate = DateTime.UtcNow 
                        };
                        await _deliveryRepository.AddAsync(delivery);
                    }
                }
            }
            notification.CreatedDate = DateTime.UtcNow;
            await _notificationRepository.UpdateAsync(notification);
        }

        public async Task SendScheduledNotificationsAsync()
        {
            var now = DateTime.UtcNow;
            var scheduled = await _notificationRepository.GetScheduledAsync(now);
            foreach (var notification in scheduled)
            {
                await SendNotificationToBuildingAsync(notification.Id);
            }
        }

        public async Task<List<NotificationDto>> GetUserNotificationsAsync(long telegramId, int page = 1)
        {
            const int pageSize = 20;
            var notifications = await _notificationRepository.GetLatestForUserAsync(telegramId, pageSize);
            return notifications.Select(n => new NotificationDto
            {
                Id = n.Id,
                Message = n.Message,
                SentDate = n.CreatedDate
            }).ToList();
        }

        public async Task<List<NotificationDto>> GetUnreadNotificationsAsync(long telegramId)
        {
            var notifications = await _notificationRepository.GetUnreadForUserAsync(telegramId);
            return notifications.Select(n => new NotificationDto
            {
                Id = n.Id,
                Message = n.Message
            }).ToList();
        }

        public async Task MarkAsReadAsync(Guid notificationId, long telegramId)
        {
            await _notificationRepository.MarkAsReadAsync(notificationId, telegramId);
        }

        public async Task<List<NotificationAdminDto>> GetAllForAdminAsync(Guid? buildingId = null)
        {
            var notifications = await _notificationRepository.GetAllWithDeliveriesAsync();
            if (buildingId.HasValue)
            {
                notifications = notifications.Where(n => n.BuildingId == buildingId.Value).ToList();
            }
            return notifications.Select(n => new NotificationAdminDto
            {
                Id = n.Id,
                Message = n.Message,
                ReadCount = n.Deliveries.Count(d => d.ReadDate != null)
            }).ToList();
        }

        public async Task RepeatToUnsentAsync(Guid notificationId)
        {
            var notification = await _notificationRepository.GetByIdWithDetailsAsync(notificationId);
            if (notification == null) throw new InvalidOperationException("Notification not found");

            var unsentDeliveries = notification.Deliveries.Where(d => d.SentDate == null).ToList();
            foreach (var delivery in unsentDeliveries)
            {
                // Логика повторной отправки (например, через бот)
                delivery.SentDate = DateTime.UtcNow;
            }
            await _notificationRepository.UpdateAsync(notification);
        }
    }
}
