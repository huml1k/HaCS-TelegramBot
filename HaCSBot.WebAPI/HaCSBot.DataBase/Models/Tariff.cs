using HaCSBot.DataBase.Enums;

namespace HaCSBot.DataBase.Models
{
	// Тарифы
	public class Tariff
	{
		public Guid Id { get; set; }
		public TariffType Type { get; set; }  // ХВС, ГВС, Электро и т.д.
		public decimal Price { get; set; }
		public DateTime ValidFrom { get; set; }
		public DateTime? ValidTo { get; set; }
		public Guid BuildingId { get; set; }  
		public Building Building { get; set; } 
	}
}
