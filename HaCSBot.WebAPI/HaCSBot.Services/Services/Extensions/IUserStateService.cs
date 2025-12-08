using HaCSBot.Services.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
		RegistrationData? GetTempRegistrationData(long telegramId);

		// Сохранить временные данные
		void SetTempRegistrationData(long telegramId, RegistrationData data);

		// Очистить временные данные
		void ClearTempRegistrationData(long telegramId);
	}

	// Временные данные для регистрации (имя, фамилия, телефон)
	public class RegistrationData
	{
		public string? FirstName { get; set; }
		public string? LastName { get; set; }
		public string? Phone { get; set; }
	}
}
