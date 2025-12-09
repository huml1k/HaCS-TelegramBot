using HaCSBot.DataBase.Enums;

namespace HaCSBot.DataBase.Models
{
	public class Notification
	{
		public Guid Id { get; set; }
		public NotificationType Type { get; set; }
		public string Title { get; set; } = string.Empty;
		public string Message { get; set; } = string.Empty;
		public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
		public DateTime? ScheduledSendDate { get; set; }  // для отложенных
		public Guid? BuildingId { get; set; }              // может быть null (на все дома)
		public Guid? BuildingMaintenanceId { get; set; }   // опционально привязано к работам
		public Guid CreatedByUserId { get; set; }          // кто создал (админ)

		public Building? Building { get; set; }
		public BuildingMaintenance? BuildingMaintenance { get; set; }
		public User CreatedByUser { get; set; } = null!;

		// Файлы
		public List<NotificationAttachment> Attachments { get; set; } = [];

		public List<NotificationDelivery> Deliveries { get; set; } = new();
	}
}
