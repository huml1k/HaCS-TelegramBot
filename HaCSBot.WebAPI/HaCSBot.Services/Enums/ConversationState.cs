namespace HaCSBot.Services.Enums
{
	public enum ConversationState
	{
		None = 0,                    // Ничего не происходит
		Registering = 1,             // Пользователь в процессе регистрации
		AwaitingMeterApartment = 2,  // Ждет выбора квартиры для показаний
		AwaitingMeterType = 3,       // Ждет выбора типа счетчика
		AwaitingMeterValue = 4,      // Ждет ввода значения счетчика
		CreatingComplaint = 5,       // В процессе создания жалобы
		AwaitingComplaintApartment = 6, // Ждет выбора квартиры для жалобы
		AwaitingComplaintCategory = 7,  // Ждет выбора категории жалобы
		AwaitingComplaintDescription = 8, // Ждет описания жалобы
		AwaitingComplaintPhoto = 9,  // Ждет фото для жалобы
		SubmittingMeterReading = 10, // Передает показания счетчиков
		EditingProfile = 11,         // Редактирует профиль
		ChangingApartment = 12,      // Меняет привязанную квартиру
		ContactingSupport = 13,      // В диалоге с поддержкой
		ViewingNotifications = 14,   // Просматривает уведомления
		PayingBill = 15,              // Оплачивает счет
		AwaitingTariffAddress = 16,
		AwaitingFirstName,
		AwaitingLastName,
		AwaitingPhone,
		AwaitingTariffApartment
	}
}
