using HaCSBot.DataBase.Enums;
using HaCSBot.Services.Enums;
using HaCSBot.Services.Services.Extensions;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static HaCSBot.Contracts.DTOs.DTOs;

namespace HaCSBot.WebAPI.Handlers
{
	public class UpdateHandler : IUpdateHandler
	{
		private readonly ITelegramBotClient _bot;
		private readonly ILogger<UpdateHandler> _logger;
		private readonly IUserStateService _userState;     
		private readonly IUserService _userService;
		private readonly ITariffService _tariffService;
		private readonly INotificationService _notificationService;
		private readonly IMeterReadingService _meterReadingService;
		private readonly IFileService _fileService;
		private readonly IComplaintService _complaintService;
		private readonly IBuildingService _buildingService;
		private readonly IApartmentService _apartmentService;


        public UpdateHandler(
			ITelegramBotClient bot,
			ILogger<UpdateHandler> logger,
			IUserStateService userState,
			IUserService userService, 
			ITariffService tariffService,
            INotificationService notificationService,
            IMeterReadingService meterReadingService,
            IFileService fileService,
            IComplaintService complaintService,
            IBuildingService buildingService,
			IApartmentService apartmentService)
		{
			_bot = bot;
			_logger = logger;
			_userState = userState;
			_userService = userService;
			_tariffService = tariffService;
			_notificationService = notificationService;
			_meterReadingService = meterReadingService;
			_fileService = fileService;
			_complaintService = complaintService;
			_buildingService = buildingService;
			_apartmentService = apartmentService;
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

            // 1. Получаем пользователя из БД
            var dbUser = await GetAuthorizedUserAsync(userId);

            // 2. Если авторизован и прислал /start — предлагаем перелогиниться
            if (dbUser is not null && text == "/start")
            {
                await OfferReLogin(chatId);
                return;
            }

            // 3. Если идёт регистрация — обрабатываем шаги регистрации
            var state = _userState.GetState(userId);
            if (state != ConversationState.None && IsRegistrationState(state))
            {
                await HandleRegistrationStep(msg, state);
                return;
            }

            // 4. Команда /start
            if (text == "/start")
            {
                if (dbUser is not null)
                {
                    await ShowMainMenu(dbUser, chatId);
                }
                else
                {
                    await SendRegistrationButton(chatId);
                }
                return;
            }

            // 5. Если НЕ авторизован — просим зарегистрироваться
            if (dbUser is null)
            {
                await SendRegistrationButton(chatId);
                return;
            }

            // 6. Если авторизован и есть активное состояние (например, ввод адреса для тарифов)
            if (state != ConversationState.None)
            {
                await HandleStateInput(msg, state);
                return;
            }

            // 7. ТОЛЬКО ТЕПЕРЬ обрабатываем кнопки главного меню
            switch (text)
            {
                case "Сообщить о проблеме":
                    await HandleReportProblem(msg, dbUser);
                    break;

                case "Передача показаний счётчиков":
                    await HandleMeterReadings(msg, dbUser);
                    break;

                case "Тарифы":
                    await HandleTariffs(msg, dbUser);
                    break;

                case "Мои жалобы":
                    await ShowMyComplaints(msg, dbUser);
                    break;

                case "Новые жалобы":
                    if (dbUser.Role == 0) // только админ
                        await ShowNewComplaints(msg, dbUser);
                    else
                        await ShowMainMenu(dbUser, chatId);
                    break;

                //case "Все жалобы":
                //    if (dbUser.Role == 0)
                //        await ShowAllComplaints(msg, dbUser);
                //    else
                //        await ShowMainMenu(dbUser, chatId);
                //    break;

                default:
                    // Неизвестное сообщение — просто показываем меню заново
                    await _bot.SendMessage(chatId, "Выберите действие из меню ниже:");
                    await ShowMainMenu(dbUser, chatId);
                    break;
            }
        }

        private bool IsRegistrationState(ConversationState state)
        {
            return state is ConversationState.AwaitingFirstName or
                   ConversationState.AwaitingLastName or
                   ConversationState.AwaitingPhone;
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

        
        private async Task HandleReportProblem(Message message, HaCSBot.DataBase.Models.User? user)
        {
            if (user == null) return;

            long chatId = message.Chat.Id;
            long userId = message.From!.Id;

            // Получаем все квартиры пользователя
            var apartments = await _apartmentService.GetByUserIdAsync(user.Id);

            if (!apartments.Any())
            {
                await _bot.SendMessage(chatId, "У вас нет зарегистрированных квартир. Обратитесь к администратору.");
                await ShowMainMenu(user, chatId);
                return;
            }

            // Если одна квартира — сразу переходим к категории
            if (apartments.Count == 1)
            {
                _userState.SetTempComplaintData(userId, new ComplaintTempData
                {
                    ApartmentId = apartments.First().Id
                });
                await AskComplaintCategory(chatId, userId);
                return;
            }

            // Если несколько — просим выбрать
            var keyboardButtons = apartments.Select(a =>
                new KeyboardButton($"{a.ApartmentNumber} — {a.Building?.StreetName ?? "Дом"} {a.Building?.BuildingNumber ?? ""}")
            ).ToArray();

            var keyboard = new ReplyKeyboardMarkup(keyboardButtons)
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = true
            };

            await _bot.SendMessage(chatId, "Выберите квартиру, по которой хотите сообщить о проблеме:", replyMarkup: keyboard);
            _userState.SetState(userId, ConversationState.AwaitingComplaintApartment);
        }

        // Обработка передачи показаний счётчиков
        private async Task HandleMeterReadings(Message message, HaCSBot.DataBase.Models.User? user)
        {
            if (user == null) return;

            long chatId = message.Chat.Id;
            long userId = message.From!.Id;

            // Получаем квартиры пользователя
            var apartments = await _apartmentService.GetByUserIdAsync(user.Id);

            if (!apartments.Any())
            {
                await _bot.SendMessage(chatId, "У вас нет зарегистрированных квартир.");
                await ShowMainMenu(user, chatId);
                return;
            }

            // Сохраняем список квартир во временных данных (для выбора)
            _userState.SetTempMeterData(userId, new MeterTempData
            {
                Apartments = apartments.ToList()
            });

            // Если одна квартира — сразу переходим к типу счётчика
            if (apartments.Count == 1)
            {
                _userState.SetTempMeterData(userId, new MeterTempData
                {
                    SelectedApartmentId = apartments.First().Id,
                    Apartments = apartments.ToList()
                });
                await ShowLastReadingsAndAskType(chatId, userId, apartments.First().Id);
                return;
            }

            // Если несколько — просим выбрать
            var keyboardButtons = apartments.Select(a =>
                new KeyboardButton($"кв. {a.ApartmentNumber} — {a.Building.StreetType.GetDisplayName()} {a.Building.StreetName}, {a.Building.BuildingNumber}")
            );

            var keyboard = new ReplyKeyboardMarkup(keyboardButtons)
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = true
            };

            await _bot.SendMessage(chatId, "Выберите квартиру для передачи показаний:", replyMarkup: keyboard);
            _userState.SetState(userId, ConversationState.AwaitingMeterApartment);
        }

        // Обработка просмотра тарифов
        private async Task HandleTariffs(Message message, HaCSBot.DataBase.Models.User? user)
        {
            if (user == null) return;

            long chatId = message.Chat.Id;
            long userId = message.From!.Id;

            // Первый шаг: проспи введите адрес
            await _bot.SendMessage(chatId,
                "Введите адрес дома, чтобы посмотреть актуальные тарифы:\n\n" +
                "Пример: ул. Ленина 25\n" +
                "Или: проспект Мира, 10а");

            // Устанавливаем состояние — следующее сообщение будет адресом
            _userState.SetState(userId, ConversationState.AwaitingTariffAddress);
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

		private async Task HandleStateInput(Message msg, ConversationState state)
		{
			long userId = msg.From!.Id;
			long chatId = msg.Chat.Id;
			string? text = msg.Text?.Trim();
			var dbUser = await GetAuthorizedUserAsync(userId);

			if (dbUser == null)
			{
				await SendRegistrationButton(chatId);
				return;
			}

			switch (state)
			{
                case ConversationState.AwaitingComplaintApartment:
                    await HandleComplaintApartmentSelection(msg, dbUser);
                    break;

                case ConversationState.AwaitingComplaintCategory:
                    await HandleComplaintCategorySelection(msg, dbUser);
                    break;

                case ConversationState.AwaitingComplaintDescription:
                    await HandleComplaintDescription(msg, dbUser);
                    break;

                case ConversationState.AwaitingComplaintAttachments:
                    await HandleComplaintAttachments(msg, dbUser);
                    break;
                case ConversationState.AwaitingMeterApartment:
                    await HandleMeterApartmentSelection(msg, dbUser);
                    break;

                case ConversationState.AwaitingMeterType:
                    await HandleMeterTypeSelection(msg, dbUser);
                    break;

                case ConversationState.AwaitingMeterValue:
                    await HandleMeterValueInput(msg, dbUser);
                    break;
                case ConversationState.AwaitingTariffAddress:
					if (string.IsNullOrWhiteSpace(text))
					{
						await _bot.SendMessage(chatId, "Адрес не может быть пустым. Попробуйте ещё раз:");
						return; // оставляем в том же состоянии
					}

					await _bot.SendChatAction(chatId, ChatAction.Typing);

					var building = await _buildingService.FindBuildingByAddressAsync(text);

					if (building == null)
					{
						await _bot.SendMessage(chatId,
							"Дом по указанному адресу не найден в системе.\n\n" +
							"Проверьте правильность написания и попробуйте снова.");
						return; // оставляем в состоянии, чтобы пользователь ввёл заново
					}

					// Получаем тарифы для этого дома
					var tariffs = await _tariffService.GetCurrentTariffsAsync(building.Id);

					if (!tariffs.Any())
					{
						await _bot.SendMessage(chatId,
							$"Для дома <b>{building.FullAddress}</b> тарифы пока не установлены.",
							parseMode: ParseMode.Html);
					}
					else
					{
						var tariffsText = new StringBuilder();
						tariffsText.AppendLine($"Тарифы для дома <b>{building.FullAddress}</b>:\n");

						foreach (var tariff in tariffs)
						{
							string typeName = tariff.Type switch
							{
                                TariffType.ColdWater => "💧 Холодная вода",
                                TariffType.HotWater => "🔥 Горячая вода",
                                TariffType.WaterDisposal => "🚰 Водоотведение",
                                TariffType.Electricity => "⚡ Электроэнергия",
                                TariffType.Gas => "🔥 Газ",
                                TariffType.Heating => "🔥 Отопление",
                                TariffType.CapitalRepair => "🔨 Капитальный ремонт",
                                TariffType.MaintenanceAndRepair => "🏠 Содержание и текущий ремонт",
                                TariffType.Antenna => "📡 Антенна",
                                TariffType.WasteDisposal => "🗑️ Вывоз ТКО",
                                TariffType.Other => "📌 Прочие услуги",
                                _ => tariff.Type.ToString()
							};

							tariffsText.AppendLine($"{typeName}: <b>{tariff.Price:F2} ₽</b>");
						}

						tariffsText.AppendLine("\nТарифы актуальны на сегодня.");

						await _bot.SendMessage(chatId, tariffsText.ToString(), parseMode: ParseMode.Html);
					}

					// Сбрасываем состояние и возвращаем в меню
					_userState.SetState(userId, ConversationState.None);
					await ShowMainMenu(dbUser, chatId);
					break;

			}
		}

        private async Task HandleMeterApartmentSelection(Message msg, HaCSBot.DataBase.Models.User dbUser)
        {
            long chatId = msg.Chat.Id;
            long userId = msg.From!.Id;
            string? text = msg.Text;

            var tempData = _userState.GetTempMeterData(userId);
            if (tempData == null || !tempData.Apartments.Any()) return;

            var selected = tempData.Apartments.FirstOrDefault(a => text?.Contains(a.ApartmentNumber) == true);

            if (selected == null)
            {
                await _bot.SendMessage(chatId, "Не удалось определить квартиру. Выберите из списка:");
                return;
            }

            tempData.SelectedApartmentId = selected.Id;
            _userState.SetTempMeterData(userId, tempData);

            await ShowLastReadingsAndAskType(chatId, userId, selected.Id);
        }

        private async Task ShowLastReadingsAndAskType(long chatId, long userId, Guid apartmentId)
        {
            // Показываем последние показания
            var lastReadings = await _meterReadingService.GetLastReadingsAsync(apartmentId);

            var sb = new StringBuilder("Последние показания:\n\n");

            if (lastReadings.Any())
            {
                foreach (var r in lastReadings)
                {
                    string typeName = r.Type switch
                    {
                        MeterType.ColdWater => "💧 Холодная вода",
                        MeterType.HotWater => "🔥 Горячая вода",
                        MeterType.ElectricityDay => "⚡ Электро (день)",
                        MeterType.ElectricityNight => "⚡ Электро (ночь)",
                        MeterType.ElectricitySingle => "⚡ Электроэнергия",
                        MeterType.Gas => "🔥 Газ",
                        MeterType.Heating => "🔥 Отопление",
                        _ => r.Type.ToString()
                    };
                    sb.AppendLine($"{typeName}: <b>{r.Value}</b> ({r.Date:dd.MM.yyyy})");
                }
            }
            else
            {
                sb.AppendLine("Показания ещё не передавались.");
            }

            sb.AppendLine("\nВыберите тип счётчика для передачи нового показания:");

            var keyboard = new ReplyKeyboardMarkup(new[]
            {
                new[] { new KeyboardButton("💧 Холодная вода"), new KeyboardButton("🔥 Горячая вода") },
                new[] { new KeyboardButton("⚡ Электро (день)"), new KeyboardButton("⚡ Электро (ночь)") },
                new[] { new KeyboardButton("⚡ Электро (однотариф)"), new KeyboardButton("🔥 Газ") },
                    new[] { new KeyboardButton("🔥 Отопление") }
             })
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = true
            };

            await _bot.SendMessage(chatId, sb.ToString(), parseMode: ParseMode.Html, replyMarkup: keyboard);
            _userState.SetState(userId, ConversationState.AwaitingMeterType);
        }

        private async Task HandleMeterTypeSelection(Message msg, HaCSBot.DataBase.Models.User dbUser)
        {
            long chatId = msg.Chat.Id;
            long userId = msg.From!.Id;
            string? text = msg.Text;

            var typeMap = new Dictionary<string, MeterType>
            {
                { "Холодная вода", MeterType.ColdWater },
                { "Горячая вода", MeterType.HotWater },
                { "Электро (день)", MeterType.ElectricityDay },
                { "Электро (ночь)", MeterType.ElectricityNight },
                { "Электро (однотариф)", MeterType.ElectricitySingle },
                { "Газ", MeterType.Gas },
                { "Отопление", MeterType.Heating }
            };

            var selected = typeMap.FirstOrDefault(k => text?.Contains(k.Key) == true).Value;

            if (selected == MeterType.None)
            {
                await _bot.SendMessage(chatId, "Выберите тип из списка:");
                return;
            }

            var tempData = _userState.GetTempMeterData(userId);
            if (tempData == null || !tempData.SelectedApartmentId.HasValue) return;

            tempData.SelectedType = selected;
            _userState.SetTempMeterData(userId, tempData);

            string typeName = text!.Contains("день") ? "электроэнергия (день)" :
                              text.Contains("ночь") ? "электроэнергия (ночь)" :
                              text.Contains("однотариф") ? "электроэнергия" : text;

            await _bot.SendMessage(chatId, $"Введите текущее показание по {typeName}:\n\nУкажите число (например: 1234.5)",
                replyMarkup: new ReplyKeyboardRemove());

            _userState.SetState(userId, ConversationState.AwaitingMeterValue);
        }

        private async Task HandleMeterValueInput(Message msg, HaCSBot.DataBase.Models.User dbUser)
        {
            long chatId = msg.Chat.Id;
            long userId = msg.From!.Id;
            string? text = msg.Text?.Trim().Replace(",", ".");

            if (!decimal.TryParse(text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal value) || value < 0)
            {
                await _bot.SendMessage(chatId, "Неверный формат. Введите число (например: 1234 или 1234.5):");
                return;
            }

            var tempData = _userState.GetTempMeterData(userId);
            if (tempData == null || !tempData.SelectedApartmentId.HasValue || !tempData.SelectedType.HasValue)
            {
                await _bot.SendMessage(chatId, "Ошибка данных. Начните заново.");
                await ShowMainMenu(dbUser, chatId);
                return;
            }

            var dto = new SubmitReadingDto
            {
                ApartmentId = tempData.SelectedApartmentId.Value,
                Type = tempData.SelectedType.Value,
                Value = value
            };

            try
            {
                await _meterReadingService.SubmitMeterReadingAsync(dto, userId);

                string typeName = tempData.SelectedType switch
                {
                    MeterType.ColdWater => "холодной воды",
                    MeterType.HotWater => "горячей воды",
                    MeterType.ElectricityDay => "электроэнергии (день)",
                    MeterType.ElectricityNight => "электроэнергии (ночь)",
                    MeterType.ElectricitySingle => "электроэнергии",
                    MeterType.Gas => "газа",
                    MeterType.Heating => "отопления",
                    _ => ""
                };

                await _bot.SendMessage(chatId,
                    $"✅ Показание успешно передано!\n\n{typeName}: <b>{value}</b>\nДата: {DateTime.Now:dd.MM.yyyy}",
                    parseMode: ParseMode.Html,
                    replyMarkup: new ReplyKeyboardRemove());

                // Очистка
                _userState.ClearTempMeterData(userId);
                _userState.SetState(userId, ConversationState.None);

                await ShowMainMenu(dbUser, chatId);
            }
            catch (Exception ex)
            {
                await _bot.SendMessage(chatId, "Ошибка при сохранении показаний. Попробуйте позже.");
                _logger.LogError(ex, "Error submitting meter reading");
                await ShowMainMenu(dbUser, chatId);
            }
        }

        private async Task HandleComplaintApartmentSelection(Message msg, HaCSBot.DataBase.Models.User dbUser)
        {
            long chatId = msg.Chat.Id;
            long userId = msg.From!.Id;
            string? text = msg.Text?.Trim();

            var apartments = await _apartmentService.GetByUserIdAsync(dbUser.Id);
            var selectedApartment = apartments.FirstOrDefault(a =>
                text?.Contains(a.ApartmentNumber) == true);

            if (selectedApartment == null)
            {
                await _bot.SendMessage(chatId, "Не удалось определить квартиру. Попробуйте ещё раз:");
                return;
            }

            var tempData = _userState.GetTempComplaintData(userId) ?? new ComplaintTempData();
            tempData.ApartmentId = selectedApartment.Id;
            _userState.SetTempComplaintData(userId, tempData);

            await AskComplaintCategory(chatId, userId);
        }

        private async Task AskComplaintCategory(long chatId, long userId)
        {
            var categories = new (string Label, ComplaintCategory Category)[]
            {
                ("💡 Освещение", ComplaintCategory.Lighting),
                ("🛗 Лифт", ComplaintCategory.Elevator),
                ("🚰 Сантехника", ComplaintCategory.Plumbing),
                ("🔥 Отопление", ComplaintCategory.Heating),
                ("🚪 Домофон", ComplaintCategory.Intercom),
                ("🧹 Уборка", ComplaintCategory.Cleanliness),
                ("🗑 Мусоропровод", ComplaintCategory.WasteChute),
                ("🚗 Парковка", ComplaintCategory.Parking),
                ("🔊 Шум", ComplaintCategory.Noise),
                ("🏠 Протечка крыши", ComplaintCategory.RoofLeak),
                ("🚪 Входная дверь", ComplaintCategory.DoorEntrance),
                ("📌 Другое", ComplaintCategory.Other)
            };

            var buttonsRows = categories
                .Select(c => new KeyboardButton(c.Label))
                .Chunk(2); // возвращает IEnumerable<KeyboardButton[]>

            var keyboard = new ReplyKeyboardMarkup(buttonsRows.Select(row => row.ToArray()))
            {
                ResizeKeyboard = true
            };

            await _bot.SendMessage(chatId, "Выберите категорию проблемы:", replyMarkup: keyboard);
            _userState.SetState(userId, ConversationState.AwaitingComplaintCategory);
        }

        private async Task HandleComplaintCategorySelection(Message msg, HaCSBot.DataBase.Models.User dbUser)
        {
            long chatId = msg.Chat.Id;
            long userId = msg.From!.Id;
            string? text = msg.Text;

            var categoryMap = new Dictionary<string, ComplaintCategory>
            {
                { "Освещение", ComplaintCategory.Lighting },
                { "Лифт", ComplaintCategory.Elevator },
                { "Сантехника", ComplaintCategory.Plumbing },
                { "Отопление", ComplaintCategory.Heating },
                { "Домофон", ComplaintCategory.Intercom },
                { "Уборка", ComplaintCategory.Cleanliness },
                { "Мусоропровод", ComplaintCategory.WasteChute },
                { "Парковка", ComplaintCategory.Parking },
                { "Шум", ComplaintCategory.Noise },
                { "Протечка крыши", ComplaintCategory.RoofLeak },
                { "Входная дверь", ComplaintCategory.DoorEntrance },
                { "Другое", ComplaintCategory.Other }
            };

            var selected = categoryMap.FirstOrDefault(kvp => text?.Contains(kvp.Key) == true);

            if (selected.Key == null)
            {
                await _bot.SendMessage(chatId, "Не понял категорию. Выберите из списка:");
                return;
            }

            var tempData = _userState.GetTempComplaintData(userId) ?? new ComplaintTempData();
            tempData.Category = selected.Value;
            _userState.SetTempComplaintData(userId, tempData);

            await _bot.SendMessage(chatId, "Опишите проблему подробно:", replyMarkup: new ReplyKeyboardRemove());
            _userState.SetState(userId, ConversationState.AwaitingComplaintDescription);
        }

        private async Task HandleComplaintDescription(Message msg, HaCSBot.DataBase.Models.User dbUser)
        {
            long chatId = msg.Chat.Id;
            long userId = msg.From!.Id;
            string? description = msg.Text?.Trim();

            if (string.IsNullOrWhiteSpace(description))
            {
                await _bot.SendMessage(chatId, "Описание не может быть пустым. Напишите ещё раз:");
                return;
            }

            var tempData = _userState.GetTempComplaintData(userId);
            if (tempData == null || !tempData.ApartmentId.HasValue || !tempData.Category.HasValue)
            {
                await _bot.SendMessage(chatId, "Ошибка данных. Начните заново.");
                await ShowMainMenu(dbUser, chatId);
                return;
            }

            tempData.Description = description;
            _userState.SetTempComplaintData(userId, tempData);

            var keyboard = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton("Отправить без фото"),
                new KeyboardButton("Прикрепить фото/документы")
            })
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = true
            };

            await _bot.SendMessage(chatId,
                $"Проверьте:\n\nКвартира: {tempData.ApartmentId}\nКатегория: {tempData.Category}\nОписание: {description}\n\n" +
                "Хотите прикрепить фото или отправить так?",
                replyMarkup: keyboard);

            _userState.SetState(userId, ConversationState.AwaitingComplaintAttachments);
        }

        private async Task HandleComplaintAttachments(Message msg, HaCSBot.DataBase.Models.User dbUser)
        {
            long chatId = msg.Chat.Id;
            long userId = msg.From!.Id;
            string? text = msg.Text;

            var tempData = _userState.GetTempComplaintData(userId);
            if (tempData == null) return;

            if (text == "Отправить без фото")
            {
                await SaveComplaint(tempData, userId, chatId, dbUser);
                return;
            }

            if (text == "Прикрепить фото/документы")
            {
                await _bot.SendMessage(chatId,
                    "Отправьте фото или документы (можно несколько сообщений).\n" +
                    "Когда закончите — нажмите кнопку ниже.",
                    replyMarkup: new ReplyKeyboardMarkup(new KeyboardButton("Готово — отправить жалобу"))
                    {
                        ResizeKeyboard = true,
                        OneTimeKeyboard = true
                    });
                return;
            }

            if (text == "Готово — отправить жалобу")
            {
                await SaveComplaint(tempData, userId, chatId, dbUser);
                return;
            }

            // Обработка вложений (фото/документ)
            if (msg.Photo != null || msg.Document != null)
            {
                // Пример сохранения фото (самое большое)
                string fileId = msg.Photo?.LastOrDefault()?.FileId ?? msg.Document!.FileId;
                var type = msg.Photo != null ? AttachmentType.Photo : AttachmentType.Document;

                tempData.Attachments.Add(new AttachmentInfo
                {
                    TelegramFileId = fileId,
                    Type = type,
                    Caption = msg.Caption
                });

                _userState.SetTempComplaintData(userId, tempData);
                await _bot.SendMessage(chatId, "Прикреплено. Можете отправить ещё или нажать «Готово».");
            }
        }

        private async Task SaveComplaint(ComplaintTempData tempData, long userId, long chatId, HaCSBot.DataBase.Models.User dbUser)
        {
            var dto = new CreateComplaintDto
            {
                ApartmentId = tempData.ApartmentId!.Value,
                Category = tempData.Category!.Value,
                Description = tempData.Description!,
                Attachments = tempData.Attachments.Select(a => new AttachmentDto
                {
                    Type = a.Type,
                    TelegramFileId = a.TelegramFileId,
                    Caption = a.Caption
                }).ToList()
            };

            try
            {
                var complaintId = await _complaintService.CreateComplaintAsync(dto, userId);

                await _bot.SendMessage(chatId,
                    "✅ Жалоба успешно отправлена!\n\n" +
                    "Мы приняли её в работу. Номер заявки: <b>#" + complaintId.ToString("N").Substring(0, 8) + "</b>\n" +
                    "О результатах сообщим дополнительно.",
                    parseMode: ParseMode.Html,
                    replyMarkup: new ReplyKeyboardRemove());

                // Очистка
                _userState.ClearTempComplaintData(userId);
                _userState.SetState(userId, ConversationState.None);
                await ShowMainMenu(dbUser, chatId);
            }
            catch (Exception ex)
            {
                await _bot.SendMessage(chatId, "Ошибка при отправке жалобы. Попробуйте позже.");
                _logger.LogError(ex, "Error creating complaint");
                await ShowMainMenu(dbUser, chatId);
            }
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

                case 0: // Администратор
                    var adminKeyboard = new ReplyKeyboardMarkup(new[]
                    {
                        new KeyboardButton("Новые жалобы"),
                        new KeyboardButton("Все жалобы"),
                        new KeyboardButton("Панель управления") // в разработке
                    })
                    {
                        ResizeKeyboard = true
                    };
                    await _bot.SendMessage(chatId, "Панель администратора:", replyMarkup: adminKeyboard);
                    return;

                default: // Житель
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
                    await _bot.SendMessage(chatId, "Главное меню жителя:", replyMarkup: residentKeyboard);
                    break;
            }
        }

       

        private async Task ShowMyComplaints(Message msg, HaCSBot.DataBase.Models.User dbUser)
        {
            long chatId = msg.Chat.Id;

            var complaints = await _complaintService.GetMyComplaintsAsync(msg.From!.Id);

            if (!complaints.Any())
            {
                await _bot.SendMessage(chatId, "У вас пока нет поданных жалоб.");
                await ShowMainMenu(dbUser, chatId);
                return;
            }

            var text = new StringBuilder("📋 Ваши жалобы:\n\n");
            foreach (var c in complaints.OrderByDescending(c => c.Status == ComplaintStatus.New ? 0 : 1))
            {
                string statusEmoji = c.Status switch
                {
                    ComplaintStatus.New => "🆕 ",
                    ComplaintStatus.Accepted => "⏳ ",
                    ComplaintStatus.InProgress => "🔧 ",
                    ComplaintStatus.Resolved => "✅ ",
                    ComplaintStatus.Closed => "✅ ",
                    ComplaintStatus.Rejected => "❌ ",
                    _ => ""
                };

                text.AppendLine($"{statusEmoji}<b>#{c.Id.ToString("N").Substring(0, 8)}</b>");
                text.AppendLine($"{c.Description}");
                text.AppendLine($"Статус: <i>{GetStatusName(c.Status)}</i>\n");
            }

            await _bot.SendMessage(chatId, text.ToString(), parseMode: ParseMode.Html);
            await ShowMainMenu(dbUser, chatId);
        }

        private async Task ShowNewComplaints(Message msg, HaCSBot.DataBase.Models.User dbUser)
        {
            long chatId = msg.Chat.Id;

            var complaints = await _complaintService.GetNewComplaintsForAdminAsync(dbUser.Id);

            if (!complaints.Any())
            {
                await _bot.SendMessage(chatId, "Новых жалоб нет.");
                await ShowMainMenu(dbUser, chatId);
                return;
            }

            var text = new StringBuilder("🆕 Новые жалобы:\n\n");
            foreach (var c in complaints)
            {
                text.AppendLine($"<b>#{c.Id.ToString("N").Substring(0, 8)}</b>");
                text.AppendLine($"{c.Description}\n");
            }

            await _bot.SendMessage(chatId, text.ToString(), parseMode: ParseMode.Html);
            await ShowMainMenu(dbUser, chatId);
        }

        private string NormalizePhone(string phone)
		{
			if (string.IsNullOrEmpty(phone)) return "";
			phone = phone.Replace("+", "").Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
			if (phone.StartsWith("8")) phone = "7" + phone.Substring(1);
			if (phone.StartsWith("9") && phone.Length == 10) phone = "7" + phone;
			return phone;
		}

        private string GetStatusName(ComplaintStatus status) => status switch
        {
            ComplaintStatus.New => "Новая",
            ComplaintStatus.Accepted => "Принята",
            ComplaintStatus.InProgress => "В работе",
            ComplaintStatus.Resolved => "Решена",
            ComplaintStatus.Closed => "Закрыта",
            ComplaintStatus.Rejected => "Отклонена",
            _ => status.ToString()
        };

        private Task UnknownUpdateHandlerAsync(Update update)
        {
            _logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
            return Task.CompletedTask;
        }
    }
}
