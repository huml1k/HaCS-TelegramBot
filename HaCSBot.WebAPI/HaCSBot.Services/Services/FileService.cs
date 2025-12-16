using AutoMapper;
using HaCSBot.Contracts.DTOs;
using HaCSBot.DataBase.Enums;
using HaCSBot.DataBase.Models;
using HaCSBot.Services.Services.Extensions;
using Telegram.Bot;

namespace HaCSBot.Services.Services
{
	public class FileService : IFileService
	{
		private readonly ITelegramBotClient _bot;
		private readonly IMapper _mapper;

		public FileService(ITelegramBotClient bot, IMapper mapper)
		{
			_bot = bot;
			_mapper = mapper;
		}

		public async Task<AttachmentDto> SaveFileFromTelegramAsync(TelegramFileInputDto fileInput)
		{
			try
			{
				// Используем FileId напрямую (Telegram уже сохранил файл)
				return new AttachmentDto
				{
					Type = fileInput.Type,
					TelegramFileId = fileInput.FileId,
					Caption = fileInput.Caption
				};
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Ошибка сохранения файла: {ex.Message}");
			}
		}

		public async Task<ComplaintAttachment> SaveComplaintAttachmentAsync(TelegramFileInputDto fileInput, Guid complaintId)
		{
			var attachmentDto = await SaveFileFromTelegramAsync(fileInput);

			var complaintAttachment = _mapper.Map<ComplaintAttachment>(attachmentDto);
			complaintAttachment.ComplaintId = complaintId;

			return complaintAttachment;
		}

		public async Task<NotificationAttachment> SaveNotificationAttachmentAsync(TelegramFileInputDto fileInput, Guid notificationId)
		{
			var attachmentDto = await SaveFileFromTelegramAsync(fileInput);

			var notificationAttachment = _mapper.Map<NotificationAttachment>(attachmentDto);
			notificationAttachment.NotificationId = notificationId;

			return notificationAttachment;
		}

		public async Task SendFileAsync(SendFileDto sendDto)
		{
			try
			{
				switch (sendDto.Type)
				{
					case AttachmentType.Photo:
						await _bot.SendPhoto(
							chatId: sendDto.TelegramId,
							photo: sendDto.TelegramFileId,
							caption: sendDto.Caption);
						break;

					case AttachmentType.Document:
						await _bot.SendDocument(
							chatId: sendDto.TelegramId,
							document: sendDto.TelegramFileId,
							caption: sendDto.Caption);
						break;

					case AttachmentType.Video:
						await _bot.SendVideo(
							chatId: sendDto.TelegramId,
							video: sendDto.TelegramFileId,
							caption: sendDto.Caption);
						break;

					case AttachmentType.Voice:
						await _bot.SendVoice(
							chatId: sendDto.TelegramId,
							voice: sendDto.TelegramFileId,
							caption: sendDto.Caption);
						break;

					case AttachmentType.VideoNote:
						await _bot.SendVideoNote(
							chatId: sendDto.TelegramId,
							videoNote: sendDto.TelegramFileId);
						break;

					case AttachmentType.Animation:
						await _bot.SendAnimation(
							chatId: sendDto.TelegramId,
							animation: sendDto.TelegramFileId,
							caption: sendDto.Caption);
						break;

					case AttachmentType.Sticker:
						await _bot.SendSticker(
							chatId: sendDto.TelegramId,
							sticker: sendDto.TelegramFileId);
						break;

					default:
						throw new ArgumentException($"Неподдерживаемый тип файла: {sendDto.Type}");
				}
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Ошибка отправки файла: {ex.Message}");
			}
		}

		public async Task SendFilesAsync(long telegramId, List<AttachmentDto> attachments)
		{
			foreach (var attachment in attachments)
			{
				var sendDto = _mapper.Map<SendFileDto>(attachment);
				sendDto.TelegramId = telegramId;

				await SendFileAsync(sendDto);
				await Task.Delay(500); // Небольшая задержка между отправками
			}
		}

		//public async Task<File> GetFileInfoAsync(string fileId)
		//{
		//	return await _botClient.GetFileAsync(fileId);
		//}

		//public async Task<byte[]> DownloadFileAsync(string fileId)
		//{
		//	var file = await GetFileInfoAsync(fileId);
		//	if (file.FilePath == null)
		//		throw new InvalidOperationException("File path not available");

		//	using var memoryStream = new MemoryStream();
		//	await _botClient.DownloadFileAsync(file.FilePath, memoryStream);
		//	return memoryStream.ToArray();
		//}
	}
}
