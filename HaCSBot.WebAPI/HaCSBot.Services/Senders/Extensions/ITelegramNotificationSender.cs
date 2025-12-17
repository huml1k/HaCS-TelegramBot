using HaCSBot.Contracts.DTOs;
using HaCSBot.DataBase.Models;

namespace HaCSBot.Services.Senders.Extensions
{
    public interface ITelegramNotificationSender
    {
        Task<bool> SendNotificationAsync(long chatId, Notification notification);
        Task<bool> SendNotificationAsync(long chatId, string message, List<AttachmentDto> attachments = null);
    }
}
