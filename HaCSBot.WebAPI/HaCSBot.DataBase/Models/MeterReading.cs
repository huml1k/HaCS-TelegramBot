using HaCSBot.DataBase.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
