using HaCSBot.DataBase.Enums;
using System.ComponentModel.DataAnnotations;

namespace HaCSBot.Contracts.DTOs
{
	// Для IMeterReadingService
	public class SubmitReadingDto
	{
		[Required]
		public Guid ApartmentId { get; set; }
		[Required]
		public MeterType Type { get; set; }
		[Required]
		public decimal Value { get; set; }
	}
}
