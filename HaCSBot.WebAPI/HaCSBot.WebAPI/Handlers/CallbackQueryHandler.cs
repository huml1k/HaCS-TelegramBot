using HaCSBot.Contracts.DTOs;
using HaCSBot.Services.Enums;
using HaCSBot.Services.Services.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace HaCSBot.WebAPI.Handlers
{
	public class CallbackQueryHandler
	{
		//Обработка нажатий на inline-кнопки (CallbackQuery).
		//Обновляет состояние, вызывает сервисы для действий (например, отметить прочитанным).
		//На практике: Это для динамических кнопок в уведомлениях или меню.

		// Обработка нажатия на inline-кнопки (Регистрация / Выйти и войти заново)
		private readonly ITelegramBotClient _bot;
		private readonly IUserStateService _userState;
		private readonly IUserService _userService;

		public CallbackQueryHandler(
			ITelegramBotClient bot,
			IUserStateService userState,
			IUserService userService
			)
		{
			_bot = bot;
			_userState = userState;	
			_userService = userService;
		}
		public async Task OnCallbackQuery(CallbackQuery callbackQuery)
		{
			if (callbackQuery.Data is not ("start_registration" or "re_register"))
				return;

			long userId = callbackQuery.From.Id;
			long chatId = callbackQuery.Message!.Chat.Id;

			await _bot.AnswerCallbackQuery(callbackQuery.Id);

			// Полная очистка старого состояния и временных данных
			_userState.ClearState(userId);
			_userState.ClearTempRegistrationData(userId);

			// Если пользователь был ранее авторизован — отвязываем Telegram ID
			var oldUser = await _userService.GetCurrentUserAsync(userId);
			if (oldUser is not null)
			{
				oldUser.TelegramId = null;
				oldUser.IsAuthorizedInBot = false;
				oldUser.LastAuthorizationDate = null;
				await _userService.UpdateUserAsync(oldUser);
			}

			// Устанавливаем новое состояние: начинаем авторизацию
			_userState.SetTempRegistrationData(userId, new RegistrationTempDto());
			_userState.SetState(userId, ConversationState.AwaitingFirstName);

			await _bot.SendMessage(chatId, "Введите ваше имя:");
		}

	}
}
