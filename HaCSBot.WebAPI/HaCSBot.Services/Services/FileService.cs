using HaCSBot.DataBase.Enums;
using HaCSBot.Services.Services.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;
using static HaCSBot.Contracts.DTOs.DTOs;

namespace HaCSBot.Services.Services
{
    public class FileService : IFileService
    {
        // Предполагаем интеграцию с Telegram.Bot client
        private readonly ITelegramBotClient _botClient;

        public FileService(ITelegramBotClient botClient)
        {
            _botClient = botClient;
        }

        public async Task<AttachmentInfo> SaveFileFromTelegramAsync(long chatId, InputFile file, AttachmentType type)
        {
            //Логика загрузки файла в Telegram и получения FileId
            var uploaded = await _botClient.SendDocument(chatId, file); // Псевдокод
            return new AttachmentInfo
            {
                TelegramFileId = uploaded.Document.FileId,
                Type = type
            };
        }

        public async Task SendFileAsync(long telegramId, AttachmentInfo attachment)
        {
            // Отправка файла по FileId
            if (attachment.Type == AttachmentType.Photo)
            {
                await _botClient.SendPhoto(telegramId, attachment.TelegramFileId, attachment.Caption);
            }
            else if (attachment.Type == AttachmentType.Document)
            {
                await _botClient.SendDocument(telegramId, attachment.TelegramFileId, attachment.Caption);
            }
        }
    }
}
