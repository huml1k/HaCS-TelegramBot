using AutoMapper;
using HaCSBot.Contracts.DTOs;
using HaCSBot.DataBase.Models;
using HaCSBot.Services.Enums;
using HaCSBot.Services.Services;
using HaCSBot.Services.Services.Extensions;
using System.Numerics;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace HaCSBot.WebAPI.Handlers
{
	public class MainMenuHandler
	{
		//Показывает главное меню в зависимости от роли (жилец/админ).
		//Переключает между меню, обрабатывает базовые кнопки.
		//На практике: Здесь логика ролей и клавиатур (ReplyKeyboardMarkup).

		// + RegistrationHandler (тк возникает цикличность)Многошаговый процесс регистрации: ввод адреса, ФИО, телефона, нормализация, поиск пользователя
		private readonly ITelegramBotClient _bot;
		private readonly IUserStateService _userState;
		private readonly IMapper _mapper;
		private readonly IUserService _userService;
		private readonly IApartmentService _apartmentService;
		private readonly ILogger<UpdateHandler> _logger;

		public MainMenuHandler(
			ITelegramBotClient bot,
			IUserStateService userState,
			IMapper mapper,
			IUserService userService,
			ILogger<UpdateHandler> logger,
			IApartmentService apartmentService
			)
		{
			_bot = bot;
			_userState = userState;
			_mapper = mapper;
			_userService = userService;
			_logger = logger;
			_apartmentService = apartmentService;
		}
		public async Task ShowMainMenu(UserProfileDto userProfile, long chatId)
		{
			var role = userProfile?.Role;
			switch (role)
			{
				case null:
					await SendRegistrationButton(chatId);
					return;

				case 0: // Администратор
					var adminKeyboard = new ReplyKeyboardMarkup(new[]
					{
						new KeyboardButton("Новые жалобы"),
						new KeyboardButton("Все жалобы"),
						new KeyboardButton("Панель управления")
					})
					{
						ResizeKeyboard = true
					};
					await _bot.SendMessage(chatId, "Панель администратора:", replyMarkup: adminKeyboard);
					return;

				default: // Житель
						 // Формируем информацию о пользователе
					var userInfo = new StringBuilder();
					userInfo.AppendLine($"👤 *{userProfile.FullName}*");
					userInfo.AppendLine($"📞 Телефон: {userProfile.Phone}");

					var apartments = await _apartmentService.GetByUserIdAsync(userProfile.Id);

					// Добавляем список квартир
					if (apartments != null && apartments.Any())
					{
						userInfo.AppendLine("🏠 Ваши квартиры:");
						foreach (var apartment in apartments)
						{
							userInfo.AppendLine($"   • {apartment.BuildingAddress}, {apartment.Number}");
						}
					}
					else
					{
						userInfo.AppendLine("🏠 Квартиры не указаны");
					}
					

					var residentKeyboard = new ReplyKeyboardMarkup(new[]
					{
						new KeyboardButton("Сообщить о проблеме"),
						new KeyboardButton("Передача показаний счётчиков"),
						new KeyboardButton("Тарифы"),
						new KeyboardButton("Мои жалобы")
					})
					{
						ResizeKeyboard = true
					};

					await _bot.SendMessage(chatId, userInfo.ToString(),
						parseMode: ParseMode.Markdown,
						replyMarkup: residentKeyboard);
					break;
			}
		}

		public async Task ProcessRegistrationFinalStep(long userId, long chatId, RegistrationTempDto data)
		{
			// === ВАЖНО: Здесь будет проверка по базе данных ===
			// Ищем пользователя с таким ФИО + телефоном
			var userFromDb = await _userService.FindByPhoneAsync(data.Phone!);

			if (userFromDb is null)
			{
				await _bot.SendMessage(chatId,
					"Ошибка авторизации!\n\nПользователь с такими данными не найден в системе ЖКХ.",
					replyMarkup: new ReplyKeyboardRemove());

				_userState.ClearState(userId);
				_userState.ClearTempRegistrationData(userId);
				await SendRegistrationButton(chatId);
				return;
			}

			// Привязываем Telegram ID и авторизуем
			userFromDb.TelegramId = userId;
			userFromDb.IsAuthorizedInBot = true;
			userFromDb.LastAuthorizationDate = DateTime.UtcNow;

			await _userService.UpdateUserAsync(userFromDb);

			_userState.ClearState(userId);
			_userState.ClearTempRegistrationData(userId);

			string roleText = userFromDb.Role == 0 ? "администратор" : "житель";

			await _bot.SendMessage(chatId,
				$"Добро пожаловать, {data.FirstName}!\nВы успешно авторизованы как {roleText} ✅",
				replyMarkup: new ReplyKeyboardRemove());

			var userDto = _mapper.Map<UserProfileDto>(userFromDb);
			await ShowMainMenu(userDto, chatId);
		}

		public async Task HandleRegistrationStep(Message msg, ConversationState state)
		{
			long userId = msg.From!.Id;
			long chatId = msg.Chat.Id;
			string? text = msg.Text?.Trim();

			// Получаем текущие временные данные (они уже есть, т.к. мы их создали на старте)
			var tempData = _userState.GetTempRegistrationData(userId)
						   ?? new RegistrationTempDto(); // на всякий случай

			switch (state)
			{
				case ConversationState.AwaitingFirstName:
					if (string.IsNullOrWhiteSpace(text))
					{
						await _bot.SendMessage(chatId, "Имя не может быть пустым. Введите ещё раз:");
						return;
					}

					tempData.FirstName = text;
					await _bot.SendMessage(chatId, "Теперь введите фамилию:");
					_userState.SetState(userId, ConversationState.AwaitingLastName);

					// Сохраняем только данные, состояние НЕ трогаем
					_userState.SetTempRegistrationData(userId, tempData);
					return; // важно — выходим, чтобы не выполнять код ниже

				case ConversationState.AwaitingLastName:
					if (string.IsNullOrWhiteSpace(text))
					{
						await _bot.SendMessage(chatId, "Фамилия не может быть пустой. Введите ещё раз:");
						return;
					}

					tempData.LastName = text;

					var phoneKeyboard = new ReplyKeyboardMarkup(
						KeyboardButton.WithRequestContact("Отправить номер телефона"))
					{
						ResizeKeyboard = true,
						OneTimeKeyboard = true
					};

					await _bot.SendMessage(chatId, "Отправьте ваш номер телефона:", replyMarkup: phoneKeyboard);
					_userState.SetState(userId, ConversationState.AwaitingPhone);

					_userState.SetTempRegistrationData(userId, tempData);
					return;

				case ConversationState.AwaitingPhone:
					string phone = msg.Contact?.PhoneNumber?.Trim() ?? text ?? "";

					if (string.IsNullOrEmpty(phone) || phone.Length < 11)
					{
						await _bot.SendMessage(chatId, "Неверный формат телефона. Попробуйте ещё раз:");
						return;
					}

					tempData.Phone = phone;

					// Здесь уже не нужно SetTempRegistrationData — данные и так сохранены выше
					// Переходим сразу к финальному шагу
					await ProcessRegistrationFinalStep(userId, chatId, tempData);
					return;
			}
		}

		public async Task SendRegistrationButton(long chatId)
		{
			var keyboard = new InlineKeyboardMarkup(
				InlineKeyboardButton.WithCallbackData("Регистрация", "start_registration"));

			await _bot.SendMessage(
				chatId,
				"Добро пожаловать в бот ЖКХ!\n\nДля начала работы необходимо зарегистрироваться:",
				replyMarkup: keyboard);
		}

		public async Task OfferReLogin(long chatId)
		{
			var keyboard = new InlineKeyboardMarkup(
				InlineKeyboardButton.WithCallbackData("Выйти и зарегистрироваться заново", "re_register"));

			await _bot.SendMessage(
				chatId,
				"Вы уже авторизованы в системе.\n\nХотите выйти и войти под другим аккаунтом?",
				replyMarkup: keyboard);
		}

		//public string NormalizePhone(string phone)
		//{
		//	if (string.IsNullOrEmpty(phone)) return "";
		//	phone = phone.Replace("+", "").Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
		//	if (phone.StartsWith("8")) phone = "7" + phone.Substring(1);
		//	if (phone.StartsWith("9") && phone.Length == 10) phone = "7" + phone;
		//	return phone;
		//}

		//public async Task<HaCSBot.DataBase.Models.User?> FindUserInDatabase(string phone)
		//{
		//	try
		//	{
		//		return await _userService.FindByPhoneAsync(phone);
		//	}
		//	catch (Exception ex)
		//	{
		//		_logger.LogError(ex, "Ошибка поиска пользователя при авторизации");
		//		return null;
		//	}
		//}

		public bool IsRegistrationState(ConversationState state)
		{
			return state == ConversationState.Registering || 
				   state is ConversationState.AwaitingFirstName or
				   ConversationState.AwaitingLastName or
				   ConversationState.AwaitingPhone;
		}
	}
}
