using HaCSBot.DataBase.Enums;
using HaCSBot.DataBase.Models;
using HaCSBot.Services.Enums;
using static HaCSBot.Contracts.DTOs.DTOs;

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

        ComplaintTempData? GetTempComplaintData(long telegramId);
        void SetTempComplaintData(long telegramId, ComplaintTempData data);
        void ClearTempComplaintData(long telegramId);

        MeterTempData? GetTempMeterData(long telegramId);
        void SetTempMeterData(long telegramId, MeterTempData data);
        void ClearTempMeterData(long telegramId);
    }

	// Временные данные для регистрации (имя, фамилия, телефон)
	public class RegistrationData
	{
		public string? FirstName { get; set; }
		public string? LastName { get; set; }
		public string? Phone { get; set; }
	}

    public class ComplaintTempData
    {
        public Guid? ApartmentId { get; set; }
        public ComplaintCategory? Category { get; set; }
        public string? Description { get; set; }
        public List<AttachmentInfo> Attachments { get; set; } = new();
    }

    public class MeterTempData
    {
        public List<Apartment> Apartments { get; set; } = new();
        public Guid? SelectedApartmentId { get; set; }
        public MeterType? SelectedType { get; set; }
    }
}
