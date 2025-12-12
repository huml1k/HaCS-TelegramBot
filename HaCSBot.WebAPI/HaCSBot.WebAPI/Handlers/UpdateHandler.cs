using HaCSBot.Services.Enums;
using HaCSBot.Services.Services.Extensions;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;

namespace HaCSBot.WebAPI.Handlers
{
	public class UpdateHandler : IUpdateHandler
	{
		private readonly ITelegramBotClient _bot;
		private readonly ILogger<UpdateHandler> _logger;
		private readonly IUserStateService _userState;     
		private readonly IUserService _userService;

		public UpdateHandler(
			ITelegramBotClient bot,
			ILogger<UpdateHandler> logger,
			IUserStateService userState,
			IUserService userService)
		{
			_bot = bot;
			_logger = logger;
			_userState = userState;
			_userService = userService;
		}

        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            _logger.LogInformation("HandleError: {Exception}", exception);
            if (exception is RequestException)
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await (update switch
            {
                { Message: { } message } => OnMessage(message),
                { EditedMessage: { } message } => OnMessage(message),
                { CallbackQuery: { } callbackQuery } => OnCallbackQuery(callbackQuery),
                { InlineQuery: { } inlineQuery } => OnInlineQuery(inlineQuery),
                { ChosenInlineResult: { } chosenInlineResult } => OnChosenInlineResult(chosenInlineResult),
                _ => UnknownUpdateHandlerAsync(update)
            });
        }

		private async Task OnMessage(Message msg)
		{
			if (msg.From is null || msg.Chat is null) return;

			long userId = msg.From.Id;
			long chatId = msg.Chat.Id;
			string? text = msg.Text?.Trim();

			_logger.LogInformation("Message from {UserId}: {Text}", userId, text);

			// 1. Проверяем, авторизован ли уже пользователь
			var dbUser = await GetAuthorizedUserAsync(userId);

			// Если авторизован и прислал /start — предлагаем перелогиниться
			if (dbUser is not null && text == "/start")
			{
				await OfferReLogin(chatId);
				return;
			}

			// 2. Если идёт процесс регистрации — обрабатываем шаги
			var state = _userState.GetState(userId);
			if (state != ConversationState.None)
			{
				await HandleRegistrationStep(msg, state);
				return;
			}

			// 3. Основная команда /start
			if (text == "/start")
			{
				if (dbUser is not null)
				{
					await ShowMainMenu(dbUser, chatId); // уже авторизован — сразу меню
				}
				else
				{
					await SendRegistrationButton(chatId); // не авторизован — кнопка "Регистрация"
				}
				return;
			}

			// 4. Если авторизован — показываем меню при любом сообщении
			if (dbUser is not null)
			{
				await ShowMainMenu(dbUser, chatId);
				return;
			}

			// 5. Во всех остальных случаях — просим зарегистрироваться
			await SendRegistrationButton(chatId);
		}

		// Проверка — уже авторизован ли этот Telegram ID
		private async Task<DataBase.Models.User?> GetAuthorizedUserAsync(long telegramId)
		{
			return await _userService.GetByTelegramIdAsync(telegramId);
		}

		private async Task SendRegistrationButton(long chatId)
		{
			var keyboard = new InlineKeyboardMarkup(
				InlineKeyboardButton.WithCallbackData("Регистрация", "start_registration"));

			await _bot.SendMessage(
				chatId,
				"Добро пожаловать в бот ЖКХ!\n\nДля начала работы необходимо зарегистрироваться:",
				replyMarkup: keyboard);
		}

		private async Task OfferReLogin(long chatId)
		{
			var keyboard = new InlineKeyboardMarkup(
				InlineKeyboardButton.WithCallbackData("Выйти и зарегистрироваться заново", "re_register"));

			await _bot.SendMessage(
				chatId,
				"Вы уже авторизованы в системе.\n\nХотите выйти и войти под другим аккаунтом?",
				replyMarkup: keyboard);
		}

		// Обработка нажатия на inline-кнопки (Регистрация / Выйти и войти заново)
		private async Task OnCallbackQuery(CallbackQuery callbackQuery)
		{
			if (callbackQuery.Data is not ("start_registration" or "re_register"))
				return;

			long userId = callbackQuery.From.Id;
			long chatId = callbackQuery.Message!.Chat.Id;

			await _bot.AnswerCallbackQuery(callbackQuery.Id);

			// Сбрасываем старое состояние и данные
			_userState.ClearState(userId);
			_userState.ClearTempRegistrationData(userId);

			// Если был авторизован — снимаем авторизацию в БД
			var oldUser = await GetAuthorizedUserAsync(userId);
			if (oldUser is not null)
			{
				oldUser.TelegramId = null;
				oldUser.IsAuthorizedInBot = false;
				oldUser.LastAuthorizationDate = null;
				// Сохрани изменения в БД — пока временно через репозиторий
				// await _userRepository.Update(oldUser);  ← добавишь потом
			}

			await _bot.SendMessage(chatId, "Введите ваше имя:");
			_userState.SetState(userId, ConversationState.AwaitingFirstName);

			// Создаём пустые временные данные
			_userState.SetTempRegistrationData(userId, new RegistrationData());
		}

		// Основной метод пошаговой регистрации
		private async Task HandleRegistrationStep(Message msg, ConversationState state)
		{
			long userId = msg.From!.Id;
			long chatId = msg.Chat.Id;
			string? text = msg.Text?.Trim();

			var tempData = _userState.GetTempRegistrationData(userId) ?? new RegistrationData();

			switch (state)
			{
				case ConversationState.AwaitingFirstName:
					tempData.FirstName = text;
					await _bot.SendMessage(chatId, "Теперь введите фамилию:");
					_userState.SetState(userId, ConversationState.AwaitingLastName);
					break;

				case ConversationState.AwaitingLastName:
					tempData.LastName = text;

					var phoneKeyboard = new ReplyKeyboardMarkup(
						KeyboardButton.WithRequestContact("Отправить номер телефона"))
					{
						ResizeKeyboard = true,
						OneTimeKeyboard = true
					};

					await _bot.SendMessage(chatId, "Отправьте ваш номер телефона:", replyMarkup: phoneKeyboard);
					_userState.SetState(userId, ConversationState.AwaitingPhone);
					break;

				case ConversationState.AwaitingPhone:
					string phone = msg.Contact?.PhoneNumber?.Trim() ?? text ?? "";

					// Приводим к единому формату (например, уберём +7 → 7, 8 → 7 и т.п.)
					phone = NormalizePhone(phone);

					if (string.IsNullOrEmpty(phone) || phone.Length < 11)
					{
						await _bot.SendMessage(chatId, "Неверный формат телефона. Попробуйте ещё раз:");
						return;
					}

					tempData.Phone = phone;
					_userState.SetTempRegistrationData(userId, tempData);

					await ProcessRegistrationFinalStep(userId, chatId, tempData);
					return;
			}

			_userState.SetTempRegistrationData(userId, tempData);
		}

		private async Task ProcessRegistrationFinalStep(long userId, long chatId, RegistrationData data)
		{
			// === ВАЖНО: Здесь будет проверка по базе данных ===
			// Ищем пользователя с таким ФИО + телефоном
			var userFromDb = await FindUserInDatabase(data.FirstName!, data.LastName!, data.Phone!);

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

			await ShowMainMenu(userFromDb, chatId);
		}

		// Поиск пользователя при регистрации
		private async Task<HaCSBot.DataBase.Models.User?> FindUserInDatabase(string firstName, string lastName, string phone)
		{
			try
			{
				return await _userService.FindByPersonalDataAsync(firstName, lastName, phone);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Ошибка поиска пользователя при авторизации");
				return null;
			}
		}

		private async Task ShowMainMenu(HaCSBot.DataBase.Models.User? user, long chatId)
		{
            var role = user?.Role;

            switch (role)
            {
                case null:
                    await SendRegistrationButton(chatId);
                    return;
                case 0:
                    await _bot.SendMessage(chatId, "Вы администратор\n\nПанель управления в разработке...");
                    return;
                default:
                    var keyboard = new ReplyKeyboardMarkup(new[]
                    {
						new KeyboardButton("Сообщить о проблеме"),
						new KeyboardButton("Передача показаний счётчиков"),
						new KeyboardButton("Тарифы")
					})
                    {
                        ResizeKeyboard = true
                    };
                    await _bot.SendMessage(chatId, "Главное меню жителя:", replyMarkup: keyboard);
                    break;
            }
        }

		private string NormalizePhone(string phone)
		{
			if (string.IsNullOrEmpty(phone)) return "";
			phone = phone.Replace("+", "").Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
			if (phone.StartsWith("8")) phone = "7" + phone.Substring(1);
			if (phone.StartsWith("9") && phone.Length == 10) phone = "7" + phone;
			return phone;
		}


        #region Inline Mode

        private async Task OnInlineQuery(InlineQuery inlineQuery)
        {
            _logger.LogInformation("Received inline query from: {InlineQueryFromId}", inlineQuery.From.Id);

            InlineQueryResult[] results = [ // displayed result
                new InlineQueryResultArticle("1", "Telegram.Bot", new InputTextMessageContent("hello")),
            new InlineQueryResultArticle("2", "is the best", new InputTextMessageContent("world"))
            ];
            await _bot.AnswerInlineQuery(inlineQuery.Id, results, cacheTime: 0, isPersonal: true);
        }

        private async Task OnChosenInlineResult(ChosenInlineResult chosenInlineResult)
        {
            _logger.LogInformation("Received inline result: {ChosenInlineResultId}", chosenInlineResult.ResultId);
            await _bot.SendMessage(chosenInlineResult.From.Id, $"You chose result with Id: {chosenInlineResult.ResultId}");
        }

        #endregion

        private Task UnknownUpdateHandlerAsync(Update update)
        {
            _logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
            return Task.CompletedTask;
        }
    }
}
