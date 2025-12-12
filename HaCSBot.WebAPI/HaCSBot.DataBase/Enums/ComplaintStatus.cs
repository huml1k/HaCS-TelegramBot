namespace HaCSBot.DataBase.Enums
{
	public enum ComplaintStatus
	{
		New = 0,
		Accepted = 1,         // Принята в работу
		InProgress = 2,
		Resolved = 3,         // Решена
		Closed = 4,           // Закрыта (с отзывом жильца)
		Rejected = 5          // Отклонена (например, не по адресу)
	}
}
