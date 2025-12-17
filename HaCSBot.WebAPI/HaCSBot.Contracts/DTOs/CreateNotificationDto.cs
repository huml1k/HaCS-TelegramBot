using HaCSBot.DataBase.Enums;
using System.ComponentModel.DataAnnotations;

namespace HaCSBot.Contracts.DTOs
{
	// Для INotificationService
	public class CreateNotificationDto
	{
		[Required]
		public NotificationType Type { get; set; }
		[Required]
        public string? Title { get; set; }
        [Required]
		public string Message { get; set; } = string.Empty;
		public Guid? BuildingId { get; set; }
		public Guid? BuildingMaintenanceId { get; set; }
		public DateTime? ScheduledSendDate { get; set; }
        public List<AttachmentDto> Attachments { get; set; } = new List<AttachmentDto>();
	}
}
