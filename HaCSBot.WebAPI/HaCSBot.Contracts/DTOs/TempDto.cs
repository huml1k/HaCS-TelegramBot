using HaCSBot.DataBase.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaCSBot.Contracts.DTOs
{
	public class RegistrationTempDto
	{
		public string? FirstName { get; set; }
		public string? LastName { get; set; }
		public string? MiddleName { get; set; }
		public string? Phone { get; set; }
		public string? ApartmentAddress { get; set; }
		public int CurrentStep { get; set; } = 1;
		public Guid? FoundUserId { get; set; }
	}

	public class ComplaintTempDto
	{
		public Guid? SelectedApartmentId { get; set; }
		public ComplaintCategory? SelectedCategory { get; set; }
		public string? Description { get; set; }
		public List<AttachmentDto> Attachments { get; set; } = new();
		public int CurrentStep { get; set; } = 1;
		public List<ApartmentDto> Apartments { get; set; } = new();
	}

	public class MeterReadingTempDto
	{
		public Guid? SelectedApartmentId { get; set; }
		public MeterType? SelectedType { get; set; }
		public decimal? EnteredValue { get; set; }
		public int CurrentStep { get; set; } = 1;
		public List<ApartmentDto> Apartments { get; set; } = new();
	}
}
