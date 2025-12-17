using HaCSBot.Contracts.DTOs;
using HaCSBot.DataBase.Enums;
using HaCSBot.DataBase.Models;
using HaCSBot.Services.Senders.Extensions;
using Microsoft.Extensions.Logging;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace HaCSBot.Services.Services
{
    public class TelegramNotificationSender : ITelegramNotificationSender
    {
        private readonly ITelegramBotClient _bot;
        private readonly ILogger<TelegramNotificationSender> _logger;

        public TelegramNotificationSender(
            ITelegramBotClient bot,
            ILogger<TelegramNotificationSender> logger)
        {
            _bot = bot;
            _logger = logger;
        }

        public async Task<bool> SendNotificationAsync(long chatId, string message, List<AttachmentDto> attachments = null)
        {
            try
            {
                await _bot.SendMessage(chatId, message, parseMode: ParseMode.Markdown);
                _logger.LogInformation($"Уведомление отправлено пользователю {chatId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка отправки уведомления пользователю {chatId}");
                return false;
            }
        }

        public async Task<bool> SendNotificationAsync(long chatId, Notification notification)
        {
            var message = FormatNotificationMessage(notification);
            return await SendNotificationAsync(chatId, message);
        }

        private string FormatNotificationMessage(Notification notification)
        {
            var sb = new StringBuilder();

            // Заголовок с иконкой типа
            sb.AppendLine($"📢 *{GetNotificationTypeEmoji(notification.Type)} {notification.Title}*");
            sb.AppendLine();

            // Сообщение
            sb.AppendLine(notification.Message);
            sb.AppendLine();

            // Дата создания
            sb.AppendLine($"_{notification.CreatedDate:dd.MM.yyyy HH:mm}_");

            return sb.ToString();
        }

        private string GetNotificationTypeEmoji(NotificationType type)
        {
            return type switch
            {
                NotificationType.PlannedMaintenance => "🛠",
                NotificationType.EmergencyShutdown => "🚨",
                NotificationType.ResourceShutdown => "💧",
                NotificationType.GeneralAnnouncement => "📢",
                NotificationType.TariffChange => "💰",
                NotificationType.PaymentReminder => "💸",
                NotificationType.MeterReadingReminder => "📊",
                NotificationType.MeetingAnnouncement => "🏠",
                NotificationType.ComplaintStatusUpdate => "✅",
                NotificationType.Other => "📌",
                _ => "📌"
            };
        }

        private string GetNotificationTypeText(NotificationType type)
        {
            return type switch
            {
                NotificationType.PlannedMaintenance => "Плановые работы",
                NotificationType.EmergencyShutdown => "Аварийное отключение",
                NotificationType.ResourceShutdown => "Отключение ресурсов",
                NotificationType.GeneralAnnouncement => "Общее объявление",
                NotificationType.TariffChange => "Изменение тарифов",
                NotificationType.PaymentReminder => "Напоминание об оплате",
                NotificationType.MeterReadingReminder => "Напоминание о показаниях",
                NotificationType.MeetingAnnouncement => "Собрание жильцов",
                NotificationType.ComplaintStatusUpdate => "Статус заявки",
                NotificationType.Other => "Прочие",
                _ => "Уведомление"
            };
        }
    }
}