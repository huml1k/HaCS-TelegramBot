namespace HaCSBot.Contracts.DTOs
{
	public class NotificationDto
	{
		public Guid Id { get; set; }
		public string Message { get; set; } = string.Empty;
		public DateTime? SentDate { get; set; }
	}
}
