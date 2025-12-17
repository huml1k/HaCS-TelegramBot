using AutoMapper;
using HaCSBot.Contracts.DTOs;
using HaCSBot.DataBase.Enums;
using HaCSBot.DataBase.Models;
using HaCSBot.DataBase.Repositories.Extensions;
using HaCSBot.Services.Senders.Extensions;
using HaCSBot.Services.Services.Extensions;
using Microsoft.Extensions.Logging;

namespace HaCSBot.Services.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IBuildingRepository _buildingRepository;
        private readonly IApartmentRepository _apartmentRepository;
        private readonly IUserRepository _userRepository;
        private readonly INotificationDeliveryRepository _deliveryRepository;
        private readonly ITelegramNotificationSender _telegramSender;
        private readonly IMapper _mapper;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            INotificationRepository notificationRepository,
            IBuildingRepository buildingRepository,
            IApartmentRepository apartmentRepository,
            IUserRepository userRepository,
            INotificationDeliveryRepository deliveryRepository,
            ITelegramNotificationSender telegramSender,
            IMapper mapper,
            ILogger<NotificationService> logger)
        {
            _notificationRepository = notificationRepository;
            _buildingRepository = buildingRepository;
            _apartmentRepository = apartmentRepository;
            _userRepository = userRepository;
            _deliveryRepository = deliveryRepository;
            _telegramSender = telegramSender;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Guid> CreateNotificationAsync(CreateNotificationDto dto, Guid adminId)
        {
            var notification = _mapper.Map<Notification>(dto);
            notification.CreatedByUserId = adminId;
            notification.CreatedDate = DateTime.UtcNow;

            // Конвертируем Attachments
            notification.Attachments = dto.Attachments?.Select(a => new NotificationAttachment
            {
                Type = a.Type,
                TelegramFileId = a.TelegramFileId,
                Caption = a.Caption
            }).ToList() ?? new List<NotificationAttachment>();

            await _notificationRepository.AddAsync(notification);

            // Если не запланировано - сразу отправляем
            if (!dto.ScheduledSendDate.HasValue || dto.ScheduledSendDate <= DateTime.UtcNow)
            {
                await SendNotificationToBuildingAsync(notification.Id);
            }

            return notification.Id;
        }

        public async Task SendNotificationToBuildingAsync(Guid notificationId)
        {
            var notification = await _notificationRepository.GetByIdWithDetailsAsync(notificationId);
            if (notification == null)
                throw new InvalidOperationException("Notification not found");

            List<long> telegramUserIds;

            if (notification.BuildingId.HasValue)
            {
                // Отправляем конкретному дому
                var apartments = await _apartmentRepository.GetApartmentsByBuildingIdAsync(notification.BuildingId.Value);
                var userIds = apartments
                    .Where(a => a.UserId != null)
                    .Select(a => a.UserId)
                    .Distinct()
                    .ToList();

                // Получаем всех пользователей за один запрос
                var users = await _userRepository.GetAllAsync();
                telegramUserIds = users
                    .Where(u => u.TelegramId != null && u.IsAuthorizedInBot)
                    .Select(u => u.TelegramId!.Value)
                    .ToList();
            }
            else
            {
                // Отправляем всем пользователям
                var allUsers = await _userRepository.GetAllAsync();
                telegramUserIds = allUsers
                    .Select(u => u.TelegramId!.Value)
                    .ToList();
            }

            _logger.LogInformation($"Отправка уведомления {notificationId} {telegramUserIds.Count} пользователям");

            var successfulDeliveries = 0;
            var failedDeliveries = 0;

            foreach (var telegramId in telegramUserIds)
            {
                try
                {
                    // Отправляем уведомление через Telegram
                    var sent = await _telegramSender.SendNotificationAsync(telegramId, notification);

                    // Создаем запись о доставке
                    var delivery = new NotificationDelivery
                    {
                        Id = Guid.NewGuid(),
                        NotificationId = notificationId,
                        TelegramUserId = telegramId,
                        SentDate = DateTime.UtcNow,
                        DeliveredDate = sent ? DateTime.UtcNow : null,
                        ReadDate = null
                    };

                    await _deliveryRepository.AddAsync(delivery);

                    if (sent)
                        successfulDeliveries++;
                    else
                        failedDeliveries++;

                    // Небольшая задержка, чтобы не перегружать Telegram API
                    await Task.Delay(100);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Ошибка отправки уведомления пользователю {telegramId}");
                    failedDeliveries++;

                    // Создаем запись о неудачной доставке
                    var delivery = new NotificationDelivery
                    {
                        Id = Guid.NewGuid(),
                        NotificationId = notificationId,
                        TelegramUserId = telegramId,
                        SentDate = DateTime.UtcNow,
                        DeliveredDate = null,
                        ReadDate = null
                    };

                    await _deliveryRepository.AddAsync(delivery);
                }
            }

            _logger.LogInformation($"Уведомление {notificationId} отправлено: успешно {successfulDeliveries}, неудачно {failedDeliveries}, всего {telegramUserIds.Count}");
        }

        public async Task SendScheduledNotificationsAsync()
        {
            var now = DateTime.UtcNow;
            var scheduled = await _notificationRepository.GetScheduledAsync(now);

            _logger.LogInformation($"Найдено {scheduled.Count} запланированных уведомлений для отправки");

            foreach (var notification in scheduled)
            {
                try
                {
                    await SendNotificationToBuildingAsync(notification.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Ошибка отправки запланированного уведомления {notification.Id}");
                }
            }
        }

        public async Task<List<NotificationDto>> GetUserNotificationsAsync(long telegramId, int page = 1)
        {
            const int pageSize = 20;
            var notifications = await _notificationRepository.GetLatestForUserAsync(telegramId, pageSize);

            // Преобразуем в DTO
            return notifications.Select(n => new NotificationDto
            {
                Id = n.Id,
                Message = n.Message,
                SentDate = n.Deliveries?
                    .FirstOrDefault(d => d.TelegramUserId == telegramId)?
                    .SentDate ?? n.CreatedDate
            }).ToList();
        }

        public async Task<List<NotificationDto>> GetUnreadNotificationsAsync(long telegramId)
        {
            var notifications = await _notificationRepository.GetUnreadForUserAsync(telegramId);
            return _mapper.Map<List<NotificationDto>>(notifications);
        }

        //public async Task MarkAsReadAsync(Guid notificationId, long telegramId)
        //{
        //    await _deliveryRepository.MarkAsReadAsync(notificationId, telegramId);
        //}

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
                delivery.SentDate = DateTime.UtcNow;
            }

            await _notificationRepository.UpdateAsync(notification);
        }
    }
}