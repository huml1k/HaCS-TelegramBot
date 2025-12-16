using AutoMapper;
using HaCSBot.DataBase.Enums;
using HaCSBot.DataBase.Models;

namespace HaCSBot.Contracts.DTOs
{
	public class TariffDto
	{
		public Guid Id { get; set; }
		public TariffType Type { get; set; }
		public decimal Price { get; set; }
		public DateTime ValidFrom { get; set; }
		public DateTime? ValidTo { get; set; }
		public Guid BuildingId { get; set; }

		// Если нужно включать информацию о здании
		public BuildingDto? Building { get; set; }
	}
}
