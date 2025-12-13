namespace HaCSBot.DataBase.Enums
{
	public enum MeterType
	{
		ColdWater = 0,			// ХВС
		HotWater = 1,			// ГВС
		ElectricityDay = 2,		// Электроэнергия (день) — для двухтарифных
		ElectricityNight = 3,	// Электроэнергия (ночь)
		ElectricitySingle = 4,  // Электроэнергия (однотарифный)
		Gas = 5,				// Газ
		Heating = 6,		    // Отопление 
		None = 7
	}
}
