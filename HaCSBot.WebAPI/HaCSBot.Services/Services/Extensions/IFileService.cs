using HaCSBot.DataBase.Enums;
using static HaCSBot.Contracts.DTOs.DTOs;

namespace HaCSBot.Services.Services.Extensions
{
    public interface IFileService 
    {
        //public Task<AttachmentInfo> SaveFileFromTelegramAsync(IInputFile file, AttachmentType type);
        public Task SendFileAsync(long telegramId, AttachmentInfo attachment);
    }
}
