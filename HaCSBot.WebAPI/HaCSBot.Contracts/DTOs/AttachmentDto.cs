using HaCSBot.DataBase.Enums;

namespace HaCSBot.Contracts.DTOs
{
	public class AttachmentDto
	{
		public AttachmentType Type { get; set; }
		public string TelegramFileId { get; set; } = string.Empty;
		public string? Caption { get; set; }
	}

	public class TelegramFileInputDto
	{
		public long ChatId { get; set; }
		public string FileId { get; set; } = string.Empty;
		public string? FileUniqueId { get; set; }
		public string? FileName { get; set; }
		public long? FileSize { get; set; }
		public string? MimeType { get; set; }
		public AttachmentType Type { get; set; }
		public string? Caption { get; set; }
	}

	public class SendFileDto
	{
		public long TelegramId { get; set; }
		public string TelegramFileId { get; set; } = string.Empty;
		public AttachmentType Type { get; set; }
		public string? Caption { get; set; }
	}
}
