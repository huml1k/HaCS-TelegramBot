using System.ComponentModel.DataAnnotations;

namespace HaCSBot.DataBase.Enums
{
	public enum ComplaintCategory
	{
		[Display(Name = "Освещение")]
		Lighting = 0,             // Перегорела лампочка
		[Display(Name = "Лифт")]
		Elevator = 1,             // Лифт
		[Display(Name = "Сантехника")]
		Plumbing = 2,
		[Display(Name = "Отопление")]
		Heating = 3,              // Холодно/жарко в квартире
		[Display(Name = "Домофон")]
		Intercom = 4,             // Домофон
		[Display(Name = "Уборка")]
		Cleanliness = 5,          // Грязь в подъезде/лифте
		[Display(Name = "Мусоропровод")]
		WasteChute = 6,           // Мусоропровод
		[Display(Name = "Парковка")]
		Parking = 7,              // Парковка во дворе
		[Display(Name = "Шум")]
		Noise = 8,                // Шум от соседей
		[Display(Name = "Протечка крыши")]
		RoofLeak = 9,             // Протекает крыша
		[Display(Name = "Входная дверь")]
		DoorEntrance = 10,        // Дверь в подъезд
		[Display(Name = "Другое")]
		Other = 11                // Другое (с описанием)

	}

	public static class EnumExtensions
	{
		public static string GetName(this Enum enumValue)
		{
			var type = enumValue.GetType();
			var memberInfo = type.GetMember(enumValue.ToString());
			if (memberInfo.Length > 0)
			{
				var displayAttr = memberInfo[0].GetCustomAttributes(typeof(DisplayAttribute), false)
					.FirstOrDefault() as DisplayAttribute;
				if (displayAttr != null)
					return displayAttr.Name;
			}
			return enumValue.ToString(); 
		}
	}
}
