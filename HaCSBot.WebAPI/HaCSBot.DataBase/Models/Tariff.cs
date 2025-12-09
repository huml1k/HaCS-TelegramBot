using HaCSBot.DataBase.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
		public Guid? BuildingId { get; set; }  // если разные по домам
	}
}
