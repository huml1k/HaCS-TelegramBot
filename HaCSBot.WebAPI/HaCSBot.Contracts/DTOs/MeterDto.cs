using HaCSBot.DataBase.Enums;
using System.ComponentModel.DataAnnotations;

namespace HaCSBot.Contracts.DTOs
{

	public class MeterReadingDto
	{
		public MeterType Type { get; set; }
		public decimal Value { get; set; }
		public DateTime Date { get; set; }
		public string TypeName => Type switch
		{
			MeterType.ColdWater => "Холодная вода",
			MeterType.HotWater => "Горячая вода",
			MeterType.ElectricityDay => "Электроэнергия (день)",
			MeterType.ElectricityNight => "Электроэнергия (ночь)",
			MeterType.ElectricitySingle => "Электроэнергия",
			MeterType.Gas => "Газ",
			MeterType.Heating => "Отопление",
			_ => Type.ToString()
		};
	}

	public class SubmitMeterReadingDto
	{
		[Required]
		public Guid ApartmentId { get; set; }

		[Required]
		public MeterType Type { get; set; }

		[Required]
		[Range(0, 999999.99)]
		public decimal Value { get; set; }
	}
}
