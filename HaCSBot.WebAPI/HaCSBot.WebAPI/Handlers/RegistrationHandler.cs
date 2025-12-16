//using AutoMapper;
//using HaCSBot.Contracts.DTOs;
//using HaCSBot.Services.Enums;
//using HaCSBot.Services.Services;
//using HaCSBot.Services.Services.Extensions;
//using Telegram.Bot;
//using Telegram.Bot.Types;
//using Telegram.Bot.Types.ReplyMarkups;

//namespace HaCSBot.WebAPI.Handlers
//{
//	public class RegistrationHandler
//	{
//		//Многошаговый процесс регистрации: ввод адреса, ФИО, телефона, нормализация, поиск пользователя.

//		private readonly ITelegramBotClient _bot;
//		private readonly ILogger<UpdateHandler> _logger;
//		private readonly IUserStateService _userState;
//		private readonly IUserService _userService;
//		private readonly IMapper _mapper;

//		public RegistrationHandler(
//			ITelegramBotClient bot,
//			ILogger<UpdateHandler> logger,
//			IUserStateService userState,
//			IUserService userService,
//			MainMenuHandler mainMenuHandler,
//			IMapper mapper
//			)
//		{
//			_bot = bot;
//			_logger = logger;
//			_userState = userState;
//			_userService = userService;
//			_mainMenuHandler = mainMenuHandler;
//			_mapper = mapper;
//		}

//		public async Task HandleRegistrationStep(Message msg, ConversationState state)
//		{
//			long userId = msg.From!.Id;
//			long chatId = msg.Chat.Id;
//			string? text = msg.Text?.Trim();

//			var tempData = _userState.GetTempRegistrationData(userId);

//			switch (state)
//			{
//				case ConversationState.AwaitingFirstName:
//					tempData.FirstName = text;
//					await _bot.SendMessage(chatId, "Теперь введите фамилию:");
//					_userState.SetState(userId, ConversationState.AwaitingLastName);
//					break;

//				case ConversationState.AwaitingLastName:
//					tempData.LastName = text;

//					var phoneKeyboard = new ReplyKeyboardMarkup(
//						KeyboardButton.WithRequestContact("Отправить номер телефона"))
//					{
//						ResizeKeyboard = true,
//						OneTimeKeyboard = true
//					};

//					await _bot.SendMessage(chatId, "Отправьте ваш номер телефона:", replyMarkup: phoneKeyboard);
//					_userState.SetState(userId, ConversationState.AwaitingPhone);
//					break;

//				case ConversationState.AwaitingPhone:
//					string phone = msg.Contact?.PhoneNumber?.Trim() ?? text ?? "";

//					// Приводим к единому формату (например, уберём +7 → 7, 8 → 7 и т.п.)
//					phone = NormalizePhone(phone);

//					if (string.IsNullOrEmpty(phone) || phone.Length < 11)
//					{
//						await _bot.SendMessage(chatId, "Неверный формат телефона. Попробуйте ещё раз:");
//						return;
//					}

//					tempData.Phone = phone;
//					_userState.SetTempRegistrationData(userId, tempData);

//					await _mainMenuHandler.ProcessRegistrationFinalStep(userId, chatId, tempData);
//					return;
//			}

//			_userState.SetTempRegistrationData(userId, tempData);
//		}

//		public async Task SendRegistrationButton(long chatId)
//		{
//			var keyboard = new InlineKeyboardMarkup(
//				InlineKeyboardButton.WithCallbackData("Регистрация", "start_registration"));

//			await _bot.SendMessage(
//				chatId,
//				"Добро пожаловать в бот ЖКХ!\n\nДля начала работы необходимо зарегистрироваться:",
//				replyMarkup: keyboard);
//		}

//		public async Task OfferReLogin(long chatId)
//		{
//			var keyboard = new InlineKeyboardMarkup(
//				InlineKeyboardButton.WithCallbackData("Выйти и зарегистрироваться заново", "re_register"));

//			await _bot.SendMessage(
//				chatId,
//				"Вы уже авторизованы в системе.\n\nХотите выйти и войти под другим аккаунтом?",
//				replyMarkup: keyboard);
//		}

//		public string NormalizePhone(string phone)
//		{
//			if (string.IsNullOrEmpty(phone)) return "";
//			phone = phone.Replace("+", "").Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
//			if (phone.StartsWith("8")) phone = "7" + phone.Substring(1);
//			if (phone.StartsWith("9") && phone.Length == 10) phone = "7" + phone;
//			return phone;
//		}

//		public async Task<HaCSBot.DataBase.Models.User?> FindUserInDatabase(string phone)
//		{
//			try
//			{
//				return await _userService.FindByPhoneAsync(phone);
//			}
//			catch (Exception ex)
//			{
//				_logger.LogError(ex, "Ошибка поиска пользователя при авторизации");
//				return null;
//			}
//		}

//		public bool IsRegistrationState(ConversationState state)
//		{
//			return state is ConversationState.AwaitingFirstName or
//				   ConversationState.AwaitingLastName or
//				   ConversationState.AwaitingPhone;
//		}
//	}
//}
