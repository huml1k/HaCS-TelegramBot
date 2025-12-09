namespace HaCSBot.DataBase.Enums
{
    public enum NotificationType
    {
		PlannedMaintenance = 0,      // Плановые работы
		EmergencyShutdown = 1,       // Аварийное отключение
		ResourceShutdown = 2,        // Отключение воды/света/газа и т.д.
		GeneralAnnouncement = 3,     // Общее объявление
		TariffChange = 4,            // Изменение тарифов
		PaymentReminder = 5,         // Напоминание об оплате
		MeterReadingReminder = 6,    // Напоминание передать показания
		MeetingAnnouncement = 7,     // Собрание жильцов
		ComplaintStatusUpdate = 8,   // Уведомление о статусе заявки
		Other = 9                    // Прочие
	}
}
