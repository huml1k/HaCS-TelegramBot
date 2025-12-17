using AutoMapper;
using HaCSBot.Contracts.DTOs;
using HaCSBot.DataBase.Enums;
using HaCSBot.Services.Enums;
using HaCSBot.Services.Services;
using HaCSBot.Services.Services.Extensions;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace HaCSBot.WebAPI.Handlers
{

	public class MeterReadingHandler
	{
		private readonly ITelegramBotClient _bot;
		private readonly ILogger<MeterReadingHandler> _logger;
		private readonly IUserStateService _userState;
		private readonly IApartmentService _apartmentService;
		private readonly MainMenuHandler _mainMenuHandler;
		private readonly IMeterReadingService _meterReadingService;
		private readonly IMapper _mapper;
		private readonly IUserService _userService;

		public MeterReadingHandler(
			ITelegramBotClient bot,
			ILogger<MeterReadingHandler> logger,
			IUserStateService userState,
			IApartmentService apartmentService,
			MainMenuHandler mainMenuHandler,
			IMeterReadingService meterReadingService,
			IMapper mapper,
			IUserService userService)
		{
			_bot = bot;
			_logger = logger;
			_userState = userState;
			_apartmentService = apartmentService;
			_mainMenuHandler = mainMenuHandler;
			_meterReadingService = meterReadingService;
			_mapper = mapper;
			_userService = userService;
		}

		public async Task HandleMeterReadingsAsync(Message message, UserProfileDto userProfileDto)
		{
			if (userProfileDto == null) return;

			long chatId = message.Chat.Id;
			long userId = message.From!.Id;

			// Получаем квартиры пользователя через DTO
			var apartmentsDto = await _apartmentService.GetByUserIdAsync(userProfileDto.Id);


			if (!apartmentsDto.Any())
			{
				await _bot.SendMessage(chatId, "У вас нет зарегистрированных квартир.");
				await _mainMenuHandler.ShowMainMenu(userProfileDto, chatId);
				return;
			}

			// Создаем временные данные
			var tempData = new MeterReadingTempDto
			{
				Apartments = apartmentsDto.ToList()
			};

			_userState.SetTempMeterData(userId, tempData);

			// Если одна квартира — сразу переходим к типу счётчика
			if (apartmentsDto.Count == 1)
			{
				tempData.SelectedApartmentId = apartmentsDto.First().Id;
				_userState.SetTempMeterData(userId, tempData);

				await ShowLastReadingsAndAskType(chatId, userId, apartmentsDto.First().Id);
				return;
			}

			// Если несколько — просим выбрать
			await ShowApartmentSelection(chatId, apartmentsDto);
			_userState.SetState(userId, ConversationState.AwaitingMeterApartment);
		}

		public async Task HandleMeterApartmentSelection(Message msg, UserProfileDto userDto)
		{
			long chatId = msg.Chat.Id;
			long userId = msg.From!.Id;
			string? text = msg.Text;

			var tempData = _userState.GetTempMeterData(userId);
			if (tempData == null || !tempData.Apartments.Any()) return;

			// Ищем квартиру по номеру в тексте
			var selected = tempData.Apartments.FirstOrDefault(a =>
				text?.Contains(a.Number) == true ||
				text?.Contains(a.BuildingAddress) == true);

			if (selected == null)
			{
				await _bot.SendMessage(chatId, "Не удалось определить квартиру. Выберите из списка:");
				await ShowApartmentSelection(chatId, tempData.Apartments);
				return;
			}

			tempData.SelectedApartmentId = selected.Id;
			_userState.SetTempMeterData(userId, tempData);

			await ShowLastReadingsAndAskType(chatId, userId, selected.Id);
		}

		public async Task HandleMeterTypeSelection(Message msg, UserProfileDto userDto)
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
				{ "Газ", MeterType.Gas }
			};

			var selectedType = typeMap.FirstOrDefault(k => text?.Contains(k.Key) == true).Value;

			if (selectedType == MeterType.None)
			{
				await _bot.SendMessage(chatId, "Выберите тип из списка:");
				return;
			}

			var tempData = _userState.GetTempMeterData(userId);
			if (tempData == null || !tempData.SelectedApartmentId.HasValue) return;

			tempData.SelectedType = selectedType;
			_userState.SetTempMeterData(userId, tempData);

			string typeName = GetMeterTypeDisplayName(selectedType);
			await _bot.SendMessage(
				chatId,
				$"Введите текущее показание по {typeName}:\n\nУкажите число (например: 1234.5)",
				replyMarkup: new ReplyKeyboardRemove());

			_userState.SetState(userId, ConversationState.AwaitingMeterValue);
		}

		public async Task HandleMeterValueInput(Message msg, UserProfileDto userDto)
		{
			long chatId = msg.Chat.Id;
			long userId = msg.From!.Id;
			string? text = msg.Text?.Trim().Replace(",", ".");

			var userProfileDto = await _userService.GetProfileAsync(userId);
			if (!decimal.TryParse(text, System.Globalization.NumberStyles.Any,
				System.Globalization.CultureInfo.InvariantCulture, out decimal value) || value < 0)
			{
				await _bot.SendMessage(chatId,
					"Неверный формат. Введите число (например: 1234 или 1234.5):");
				return;
			}

			var tempData = _userState.GetTempMeterData(userId);
			if (tempData == null || !tempData.SelectedApartmentId.HasValue || !tempData.SelectedType.HasValue)
			{
				await _bot.SendMessage(chatId, "Ошибка данных. Начните заново.");
				await _mainMenuHandler.ShowMainMenu(userProfileDto, chatId);
				return;
			}

			var submitDto = new SubmitMeterReadingDto
			{
				ApartmentId = tempData.SelectedApartmentId.Value,
				Type = tempData.SelectedType.Value,
				Value = value
			};

			try
			{
				// Используем DTO для сохранения
				await _meterReadingService.SubmitMeterReadingAsync(submitDto, userId);

				string typeName = GetMeterTypeDisplayName(tempData.SelectedType.Value);

				await _bot.SendMessage(
					chatId,
					$"✅ Показание успешно передано!\n\n{typeName}: <b>{value}</b>\nДата: {DateTime.Now:dd.MM.yyyy}",
					parseMode: ParseMode.Html,
					replyMarkup: new ReplyKeyboardRemove());

				// Очистка
				_userState.ClearTempMeterData(userId);
				_userState.SetState(userId, ConversationState.None);

				await _mainMenuHandler.ShowMainMenu(userProfileDto, chatId);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error submitting meter reading");
				await _bot.SendMessage(chatId,
					$"Ошибка при сохранении показаний: {ex.Message}");
				await _mainMenuHandler.ShowMainMenu(userProfileDto, chatId);
			}
		}

		private async Task ShowApartmentSelection(long chatId, IEnumerable<ApartmentDto> apartments)
		{
			var keyboardButtons = apartments.Select(a =>
				new KeyboardButton($"кв. {a.Number} — {a.BuildingAddress}")
			);

			var keyboard = new ReplyKeyboardMarkup(keyboardButtons)
			{
				ResizeKeyboard = true,
				OneTimeKeyboard = true
			};

			await _bot.SendMessage(chatId, "Выберите квартиру для передачи показаний:",
				replyMarkup: keyboard);
		}

		public async Task ShowLastReadingsAndAskType(long chatId, long userId, Guid apartmentId)
		{
			// Получаем последние показания через DTO
			var lastReadings = await _meterReadingService.GetLastReadingsAsync(apartmentId);

			var sb = new StringBuilder("📊 Последние показания:\n\n");

			if (lastReadings.Any())
			{
				foreach (var reading in lastReadings)
				{
					sb.AppendLine($"• {reading.TypeName}: <b>{reading.Value}</b> ({reading.Date:dd.MM.yyyy})");
				}
			}
			else
			{
				sb.AppendLine("Показания ещё не передавались.");
			}

			sb.AppendLine("\n📝 Выберите тип счётчика для передачи нового показания:");

			var keyboard = CreateMeterTypeKeyboard();

			await _bot.SendMessage(chatId, sb.ToString(),
				parseMode: ParseMode.Html,
				replyMarkup: keyboard);

			_userState.SetState(userId, ConversationState.AwaitingMeterType);
		}

		private ReplyKeyboardMarkup CreateMeterTypeKeyboard()
		{
			return new ReplyKeyboardMarkup(new[]
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
		}

		private string GetMeterTypeDisplayName(MeterType type)
		{
			return type switch
			{
				MeterType.ColdWater => "холодной воде",
				MeterType.HotWater => "горячей воде",
				MeterType.ElectricityDay => "электроэнергии (день)",
				MeterType.ElectricityNight => "электроэнергии (ночь)",
				MeterType.ElectricitySingle => "электроэнергии",
				MeterType.Gas => "газу",
				MeterType.Heating => "отоплению",
				_ => type.ToString()
			};
		}
	}
}
