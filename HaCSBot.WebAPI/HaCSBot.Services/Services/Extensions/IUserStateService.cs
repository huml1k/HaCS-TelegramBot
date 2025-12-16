using HaCSBot.Contracts.DTOs;
using HaCSBot.DataBase.Enums;
using HaCSBot.DataBase.Models;
using HaCSBot.Services.Enums;

namespace HaCSBot.Services.Services.Extensions
{
	public interface IUserStateService
	{
		// Получить текущее состояние пользователя по Telegram ID
		ConversationState GetState(long telegramId);

		// Установить состояние пользователя
		void SetState(long telegramId, ConversationState state);

		// Очистить состояние (регистрация завершена или сброс)
		void ClearState(long telegramId);

		// Получить временные данные регистрации для пользователя
		RegistrationTempDto? GetTempRegistrationData(long telegramId);

		// Сохранить временные данные
		void SetTempRegistrationData(long telegramId, RegistrationTempDto data);

		// Очистить временные данные
		void ClearTempRegistrationData(long telegramId);

		ComplaintTempDto? GetTempComplaintData(long telegramId);
		void SetTempComplaintData(long telegramId, ComplaintTempDto data);
		void ClearTempComplaintData(long telegramId);

		MeterReadingTempDto? GetTempMeterData(long telegramId);
		void SetTempMeterData(long telegramId, MeterReadingTempDto data);
		void ClearTempMeterData(long telegramId);
	}




	public class UserStateDto
	{
		public ConversationState State { get; set; }
		public RegistrationTempDto? RegistrationData { get; set; }
		public ComplaintTempDto? ComplaintData { get; set; }
		public MeterReadingTempDto? MeterData { get; set; }
		public DateTime LastActivity { get; set; } = DateTime.UtcNow;
	}
}
