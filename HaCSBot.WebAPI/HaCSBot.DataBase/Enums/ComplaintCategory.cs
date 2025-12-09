using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaCSBot.DataBase.Enums
{
	public enum ComplaintCategory
	{
		Lighting = 0,             // Перегорела лампочка
		Elevator = 1,             // Лифт
		Plumbing = 2,             // Течёт труба, кран
		Heating = 3,              // Холодно/жарко в квартире
		Intercom = 4,             // Домофон
		Cleanliness = 5,          // Грязь в подъезде/лифте
		WasteChute = 6,           // Мусоропровод
		Parking = 7,              // Парковка во дворе
		Noise = 8,                // Шум от соседей
		RoofLeak = 9,             // Протекает крыша
		DoorEntrance = 10,        // Дверь в подъезд
		Other = 11                // Другое (с описанием)
	}
}
