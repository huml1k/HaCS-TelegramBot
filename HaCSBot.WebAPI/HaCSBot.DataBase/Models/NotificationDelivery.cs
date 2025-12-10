namespace HaCSBot.DataBase.Models
{
	// Доставка и статус прочтения 
	public class NotificationDelivery
	{
		public Guid Id { get; set; }
		public Guid NotificationId { get; set; }
		public long TelegramUserId { get; set; }     
		public DateTime? SentDate { get; set; }
		public DateTime? DeliveredDate { get; set; }
		public DateTime? ReadDate { get; set; }      // когда нажал "Прочитано"

		public Notification Notification { get; set; } = null!;
	}
}
