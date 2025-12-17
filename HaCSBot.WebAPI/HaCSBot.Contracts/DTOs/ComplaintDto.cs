using HaCSBot.DataBase.Enums;
using System.ComponentModel.DataAnnotations;

namespace HaCSBot.Contracts.DTOs
{
	public class ComplaintDto
	{
		public Guid Id { get; set; }
		public string Description { get; set; } = string.Empty;
		public List<string> Attachments { get; set; } = new List<string>();
		public ComplaintStatus Status { get; set; }

		public ComplaintCategory Category { get; set; }
	}

	public class ComplaintDetailsDto
	{
		public Guid Id { get; set; }
		public Guid ApartmentId { get; set; }
		public string ApartmentNumber { get; set; } = string.Empty;
		public string BuildingAddress { get; set; } = string.Empty;
		public ComplaintCategory Category { get; set; }
		public string Description { get; set; } = string.Empty;
		public ComplaintStatus Status { get; set; }
		public DateTime CreatedDate { get; set; }
		public DateTime? ResolvedDate { get; set; }
		public List<AttachmentDto> Attachments { get; set; } = new List<AttachmentDto>();
	}

	public class CreateComplaintDto
	{
		[Required]
		public Guid ApartmentId { get; set; }

		[Required]
		public ComplaintCategory Category { get; set; }

		[Required]
		public string Description { get; set; } = string.Empty;

		public List<AttachmentDto> Attachments { get; set; } = new List<AttachmentDto>();
	}

	public class ComplaintStatusChangeDto
	{
		[Required]
		public Guid ComplaintId { get; set; }

		[Required]
		public ComplaintStatus Status { get; set; }

		public string? AdminComment { get; set; }
	}
}
