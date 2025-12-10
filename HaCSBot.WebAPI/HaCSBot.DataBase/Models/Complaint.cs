using HaCSBot.DataBase.Enums;

namespace HaCSBot.DataBase.Models
{
	// Жалобы от жильцов
	public class Complaint
	{
		public Guid Id { get; set; }
		public Guid ApartmentId { get; set; }
		public ComplaintCategory Category { get; set; }
		public string Description { get; set; } = string.Empty;
		public ComplaintStatus Status { get; set; } = ComplaintStatus.New;
		public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
		public DateTime? ResolvedDate { get; set; }

		public List<ComplaintAttachment> Attachments { get; set; } = [];
		public Apartment Apartment { get; set; } = null!;
	}
}
