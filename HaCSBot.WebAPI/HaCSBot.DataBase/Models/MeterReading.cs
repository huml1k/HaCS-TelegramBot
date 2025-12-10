using HaCSBot.DataBase.Enums;

namespace HaCSBot.DataBase.Models
{
	// Показания счётчиков
	public class MeterReading
	{
		public Guid Id { get; set; }
		public Guid ApartmentId { get; set; }
		public MeterType Type { get; set; }  // ХВС, ГВС, Электро, Газ
		public decimal Value { get; set; }
		public DateTime ReadingDate { get; set; } = DateTime.UtcNow;
		public Guid SubmittedByUserId { get; set; }

		public Apartment Apartment { get; set; } = null!;
	}
}
