using AutoMapper;
using HaCSBot.Contracts.DTOs;
using HaCSBot.DataBase.Enums;
using HaCSBot.DataBase.Models;
using HaCSBot.Services.Enums;
using HaCSBot.Services.Services;
using HaCSBot.Services.Services.Extensions;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace HaCSBot.WebAPI.Handlers
{
	public class TariffHandler
	{
		private readonly ITelegramBotClient _bot;
		private readonly IUserStateService _userState;
		private readonly ITariffService _tariffService;
		private readonly IBuildingService _buildingService;
		private readonly IApartmentService _apartmentService;
		private readonly MainMenuHandler _mainMenuHandler;
		private readonly IMapper _mapper;

		public TariffHandler(
			ITelegramBotClient bot,
			IUserStateService userState,
			ITariffService tariffService,
			IBuildingService buildingService,
			IApartmentService apartmentService,
			MainMenuHandler mainMenuHandler,
			IMapper mapper)
		{
			_bot = bot;
			_userState = userState;
			_tariffService = tariffService;
			_buildingService = buildingService;
			_apartmentService = apartmentService;
			_mainMenuHandler = mainMenuHandler;
			_mapper = mapper;
		}

		// Обработка просмотра тарифов
		public async Task HandleTariffs(Message message, UserProfileDto user)
		{
			if (user == null) return;

			long chatId = message.Chat.Id;
			long userId = message.From!.Id;
			var apartmentsDto = await _apartmentService.GetByUserIdAsync(user.Id);

			if (apartmentsDto.Count == 1)
			{
				// Если квартира одна, сразу показываем тарифы для её дома
				var apartment = apartmentsDto.First();
				await ShowTariffsForBuilding(chatId, apartment.BuildingId);
				_userState.SetState(userId, ConversationState.None);
				await _mainMenuHandler.ShowMainMenu(user, chatId);
			}
			else if (apartmentsDto.Count > 1)
			{
				await ShowApartmentSelection(chatId, apartmentsDto);
				_userState.SetState(userId, ConversationState.AwaitingTariffApartment);
			}
			else
			{
				// Если нет привязанных квартир, запрашиваем адрес вручную
				await _bot.SendMessage(chatId,
					"Введите адрес дома для просмотра тарифов:\n\n" +
					"Пример: ул. Ленина 12 или пр. Мира 25А");
				_userState.SetState(userId, ConversationState.AwaitingTariffAddress);
			}
		}

		// Обработка выбора квартиры
		public async Task HandleTariffApartmentSelection(Message msg, UserProfileDto user)
		{
			long chatId = msg.Chat.Id;
			long userId = msg.From!.Id;
			string? text = msg.Text?.Trim();

			if (string.IsNullOrWhiteSpace(text))
			{
				await _bot.SendMessage(chatId, "Пожалуйста, выберите квартиру из списка:");
				return;
			}

			var apartmentsDto = await _apartmentService.GetByUserIdAsync(user.Id);
			var selectedApartment = apartmentsDto.FirstOrDefault(a =>
				$"кв. {a.Number} — {a.BuildingAddress}".Equals(text, StringComparison.OrdinalIgnoreCase));

			if (selectedApartment == null)
			{
				await _bot.SendMessage(chatId, "Квартира не найдена. Выберите из списка:");
				return;
			}

			await ShowTariffsForBuilding(chatId, selectedApartment.BuildingId);
			_userState.SetState(userId, ConversationState.None);
			await _mainMenuHandler.ShowMainMenu(user, chatId);
		}

		// Обработка ввода адреса (для пользователей без привязанных квартир)
		public async Task HandleTariffAddressInput(Message msg, UserProfileDto user)
		{
			long chatId = msg.Chat.Id;
			long userId = msg.From!.Id;
			string? text = msg.Text?.Trim();

			if (string.IsNullOrWhiteSpace(text))
			{
				await _bot.SendMessage(chatId, "Адрес не может быть пустым. Попробуйте ещё раз:");
				return;
			}

			await _bot.SendChatAction(chatId, ChatAction.Typing);

			var building = await _buildingService.FindBuildingByAddressAsync(text);

			if (building == null)
			{
				await _bot.SendMessage(chatId,
					"Дом по указанному адресу не найден в системе.\n\n" +
					"Проверьте правильность написания и попробуйте снова.");
				return;
			}

			await ShowTariffsForBuilding(chatId, building.Id);
			_userState.SetState(userId, ConversationState.None);
			await _mainMenuHandler.ShowMainMenu(user, chatId);
		}

		private async Task ShowTariffsForBuilding(long chatId, Guid buildingId)
		{
			var tariffs = await _tariffService.GetCurrentTariffsAsync(buildingId);

			if (!tariffs.Any())
			{
				await _bot.SendMessage(chatId,
					"Для выбранного дома тарифы пока не установлены.",
					parseMode: ParseMode.Html);
				return;
			}

			var building = await _buildingService.GetByIdAsync(buildingId);
			var tariffsText = new StringBuilder();
			tariffsText.AppendLine($"<b>Тарифы для дома:</b>\n{building?.FullAddress ?? "Неизвестный адрес"}\n");

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
					TariffType.MaintenanceAndRepair => "🏠 Содержание и ремонт",
					TariffType.Antenna => "📡 Антенна",
					TariffType.WasteDisposal => "🗑️ Вывоз ТКО",
					TariffType.Other => "📌 Прочие услуги",
					_ => tariff.Type.ToString()
				};

				tariffsText.AppendLine($"{typeName}: <b>{tariff.Price:F2} ₽</b>");
			}

			tariffsText.AppendLine("\n<i>Тарифы актуальны на текущую дату</i>");

			await _bot.SendMessage(chatId, tariffsText.ToString(),
				parseMode: ParseMode.Html);
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

			await _bot.SendMessage(chatId,
				"У вас несколько квартир. Выберите одну для просмотра тарифов:",
				replyMarkup: keyboard);
		}
	}
}
