using HaCSBot.Contracts.DTOs;
using HaCSBot.DataBase.Enums;
using HaCSBot.DataBase.Models;

namespace HaCSBot.Services.Services.Extensions
{
	public interface IFileService
	{
		Task<AttachmentDto> SaveFileFromTelegramAsync(TelegramFileInputDto fileInput);
		Task<ComplaintAttachment> SaveComplaintAttachmentAsync(TelegramFileInputDto fileInput, Guid complaintId);
		Task<NotificationAttachment> SaveNotificationAttachmentAsync(TelegramFileInputDto fileInput, Guid notificationId);
		Task SendFileAsync(SendFileDto sendDto);
		Task SendFilesAsync(long telegramId, List<AttachmentDto> attachments);
		//Task<File> GetFileInfoAsync(string fileId);
		//Task<byte[]> DownloadFileAsync(string fileId);
	}
}
