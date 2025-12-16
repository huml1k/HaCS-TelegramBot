namespace HaCSBot.Contracts.DTOs
{
	public class NotificationAdminDto
	{
		public Guid Id { get; set; }
		public string Message { get; set; } = string.Empty;
		public int ReadCount { get; set; }
	}
}
