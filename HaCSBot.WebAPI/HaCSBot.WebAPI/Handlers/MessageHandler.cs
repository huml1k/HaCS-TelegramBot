using HaCSBot.Contracts.DTOs;
using HaCSBot.Services.Enums;
using HaCSBot.Services.Services.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace HaCSBot.WebAPI.Handlers
{
	public class MessageHandler
	{
		//Логика приёма сообщений, проверка авторизации, обработка команд /start, делегирование в другие хендлеры.
		private readonly ITelegramBotClient _bot;
		private readonly IUserStateService _userState;
		private readonly IUserService _userService;
		private readonly ILogger<UpdateHandler> _logger;
		private readonly IApartmentService _apartmentService;
		private readonly MainMenuHandler _mainMenuHandler;
		private readonly StateDispatcherHandler _stateDispatcherHandler;
		private readonly MeterReadingHandler _meterReadingHandler;
		private readonly TariffHandler _tariffHandler;
		private readonly ComplaintHandler _complaintHandler;

		public MessageHandler(
			ITelegramBotClient bot,
			IUserStateService userState,
			IUserService userService,
			ILogger<UpdateHandler> logger,
			IApartmentService apartmentService,
			MainMenuHandler mainMenuHandler,
			StateDispatcherHandler stateDispatcherHandler,
			MeterReadingHandler meterReadingHandler,
			TariffHandler tariffHandler,
			ComplaintHandler complaintHandler
			)
		{
			_bot = bot;	
			_userState = userState;
			_userService = userService;
			_logger = logger;
			_apartmentService = apartmentService;
			_mainMenuHandler = mainMenuHandler;
			_stateDispatcherHandler = stateDispatcherHandler;
			_meterReadingHandler = meterReadingHandler;
			_tariffHandler = tariffHandler;
			_complaintHandler = complaintHandler;
		}


		public async Task HandleMessageAsync(Message msg)
		{
			if (msg.From is null || msg.Chat is null) return;

			long userId = msg.From.Id;
			long chatId = msg.Chat.Id;
			string? text = msg.Text?.Trim();

			_logger.LogInformation("Message from {UserId}: {Text}", userId, text);

			// 1. Получаем пользователя из БД
			var userDto = await _userService.GetUserDtoAsync(userId);
			var userProfileDto = await _userService.GetProfileAsync(userId);

			// 2. Если авторизован и прислал /start — предлагаем перелогиниться
			if (userDto is not null && text == "/start")
			{
				await _mainMenuHandler.OfferReLogin(chatId);
				return;
			}

			// 3. Если идёт регистрация — обрабатываем шаги регистрации
			var state = _userState.GetState(userId);
			if (state != ConversationState.None && _mainMenuHandler.IsRegistrationState(state))
			{
				await _mainMenuHandler.HandleRegistrationStep(msg, state);
				return;
			}

			// 4. Команда /start
			if (text == "/start")
			{
				if (userDto is not null)
				{
					await _mainMenuHandler.ShowMainMenu(userProfileDto, chatId);
				}
				else
				{
					await _mainMenuHandler.SendRegistrationButton(chatId);
				}
				return;
			}

			// 5. Если НЕ авторизован — просим зарегистрироваться
			if (userDto is null)
			{
				await _mainMenuHandler.SendRegistrationButton(chatId);
				return;
			}

			// 6. Если авторизован и есть активное состояние (например, ввод адреса для тарифов)
			if (state != ConversationState.None)
			{
				await _stateDispatcherHandler.HandleStateInput(msg, state);
				return;
			}

			// 7. ТОЛЬКО ТЕПЕРЬ обрабатываем кнопки главного меню
			switch (text)
			{
				case "Сообщить о проблеме":
					await _complaintHandler.HandleReportProblem(msg);
					break;

				case "Передача показаний счётчиков":
					await _meterReadingHandler.HandleMeterReadingsAsync(msg, userProfileDto);
					break;

				case "Тарифы":
					await _tariffHandler.HandleTariffs(msg, userProfileDto);
					break;

				case "Мои жалобы":
					await _complaintHandler.ShowMyComplaints(msg, userProfileDto);
					break;

				case "Новые жалобы":
					if (userDto.Role == 0) // только админ
						await _complaintHandler.ShowNewComplaints(msg, userProfileDto);
					else
						await _mainMenuHandler.ShowMainMenu(userProfileDto, chatId);
					break;

				//case "Все жалобы":
				//	if (dbUser.Role == 0)
				//		await _complaintHandler.ShowAllComplaints(msg, dbUser);
				//	else
				//		await _mainMenuHandler.ShowMainMenu(dbUser, chatId);
				//	break;

				default:
					// Неизвестное сообщение — просто показываем меню заново
					await _bot.SendMessage(chatId, "Выберите действие из меню ниже:");
					await _mainMenuHandler.ShowMainMenu(userProfileDto, chatId);
					break;
			}
		}
	}

}
