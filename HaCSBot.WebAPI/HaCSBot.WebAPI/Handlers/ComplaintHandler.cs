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
	public class ComplaintHandler
	{
		//Обработка жалоб — подача, просмотр своих/новых, смена статуса.
		//Вызывает ComplaintService, строит сообщения с эмодзи.
		//На практике: Это для модуля "Сообщить о проблеме" и админских "Новые жалобы".

		private readonly IApartmentService _apartmentService;
		private readonly ITelegramBotClient _bot;
		private readonly IUserStateService _userState;
		private readonly IComplaintService _complaintService;
		private readonly MainMenuHandler _mainMenuHandler;
		private readonly ILogger<UpdateHandler> _logger;

		public ComplaintHandler(
			IApartmentService apartmentService,
			ITelegramBotClient bot,
			IUserStateService userState,
			IComplaintService complaintService,
			MainMenuHandler mainMenuHandler,
			ILogger<UpdateHandler> logger
			)
		{
			_apartmentService = apartmentService;
			_bot = bot;
			_userState = userState;
			_complaintService = complaintService;
			_mainMenuHandler = mainMenuHandler;
			_logger = logger;
		}


		public async Task HandleComplaintApartmentSelection(Message msg, UserProfileDto userDto)
		{
			long chatId = msg.Chat.Id;
			long userId = msg.From!.Id;
			string? text = msg.Text?.Trim();

			var apartments = await _apartmentService.GetByUserIdAsync(userDto.Id);
			var selectedApartment = apartments.FirstOrDefault(a =>
				text?.Contains(a.Number) == true);

			if (selectedApartment == null)
			{
				await _bot.SendMessage(chatId, "Не удалось определить квартиру. Попробуйте ещё раз:");
				return;
			}

			var tempData = _userState.GetTempComplaintData(userId) ?? new ComplaintTempDto();
			tempData.SelectedApartmentId = selectedApartment.Id;
			_userState.SetTempComplaintData(userId, tempData);

			await AskComplaintCategory(chatId, userId);
		}

		public async Task AskComplaintCategory(long chatId, long userId)
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

		public async Task HandleComplaintCategorySelection(Message msg, UserProfileDto userDto)
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

			var tempData = _userState.GetTempComplaintData(userId) ?? new ComplaintTempDto();
			tempData.SelectedCategory = selected.Value;
			_userState.SetTempComplaintData(userId, tempData);

			await _bot.SendMessage(chatId, "Опишите проблему подробно:", replyMarkup: new ReplyKeyboardRemove());
			_userState.SetState(userId, ConversationState.AwaitingComplaintDescription);
		}

		public async Task HandleComplaintDescription(Message msg,UserProfileDto user)
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
			if (tempData == null || !tempData.SelectedApartmentId.HasValue || !tempData.SelectedCategory.HasValue)
			{
				await _bot.SendMessage(chatId, "Ошибка данных. Начните заново.");
				await _mainMenuHandler.ShowMainMenu(user, chatId);
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
				$"Проверьте:\n\nКвартира: {tempData.SelectedApartmentId}\nКатегория: {tempData.SelectedCategory}\nОписание: {description}\n\n" +
				"Хотите прикрепить фото или отправить так?",
				replyMarkup: keyboard);

			_userState.SetState(userId, ConversationState.AwaitingComplaintPhoto);
		}

		public async Task HandleComplaintAttachments(Message msg, UserProfileDto userDto)
		{
			long chatId = msg.Chat.Id;
			long userId = msg.From!.Id;
			string? text = msg.Text;

			var tempData = _userState.GetTempComplaintData(userId);
			if (tempData == null) return;

			if (text == "Отправить без фото")
			{
				await SaveComplaint(tempData, userId, chatId, userDto);
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
				await SaveComplaint(tempData, userId, chatId, userDto);
				return;
			}

			// Обработка вложений (фото/документ)
			if (msg.Photo != null || msg.Document != null)
			{
				// Пример сохранения фото (самое большое)
				string fileId = msg.Photo?.LastOrDefault()?.FileId ?? msg.Document!.FileId;
				var type = msg.Photo != null ? AttachmentType.Photo : AttachmentType.Document;

				tempData.Attachments.Add(new AttachmentDto
				{
					TelegramFileId = fileId,
					Type = type,
					Caption = msg.Caption
				});

				_userState.SetTempComplaintData(userId, tempData);
				await _bot.SendMessage(chatId, "Прикреплено. Можете отправить ещё или нажать «Готово».");
			}
		}

		public async Task SaveComplaint(ComplaintTempDto tempData, long userId, long chatId, UserProfileDto userDto)
		{
			var dto = new CreateComplaintDto
			{
				ApartmentId = tempData.SelectedApartmentId!.Value,
				Category = tempData.SelectedCategory!.Value,
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
					"Мы приняли её в работу. Номер заявки: <b>#" + complaintId + "</b>\n" +
					"О результатах сообщим дополнительно.",
					parseMode: ParseMode.Html,
					replyMarkup: new ReplyKeyboardRemove());

				// Очистка
				_userState.ClearTempComplaintData(userId);
				_userState.SetState(userId, ConversationState.None);
				await _mainMenuHandler.ShowMainMenu(userDto, chatId);
			}
			catch (Exception ex)
			{
				await _bot.SendMessage(chatId, "Ошибка при отправке жалобы. Попробуйте позже.");
				_logger.LogError(ex, "Error creating complaint");
				await _mainMenuHandler.ShowMainMenu(userDto, chatId);
			}
		}

		public async Task ShowMyComplaints(Message msg, UserProfileDto userDto)
		{
			long chatId = msg.Chat.Id;

			var complaints = await _complaintService.GetMyComplaintsAsync(msg.From!.Id);

			if (!complaints.Any())
			{
				await _bot.SendMessage(chatId, "У вас пока нет поданных жалоб.");
				await _mainMenuHandler.ShowMainMenu(userDto, chatId);
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
			await _mainMenuHandler.ShowMainMenu(userDto, chatId);
		}

		public async Task ShowNewComplaints(Message msg, UserProfileDto userDto)
		{
			long chatId = msg.Chat.Id;

			var complaints = await _complaintService.GetNewComplaintsForAdminAsync(userDto.Id);

			if (!complaints.Any())
			{
				await _bot.SendMessage(chatId, "Новых жалоб нет.");
				await _mainMenuHandler.ShowMainMenu(userDto, chatId);
				return;
			}

			var text = new StringBuilder("🆕 Новые жалобы:\n\n");
			foreach (var c in complaints)
			{
				text.AppendLine($"<b>#{c.Id.ToString("N").Substring(0, 8)}</b>");
				text.AppendLine($"{c.Description}\n");
			}

			await _bot.SendMessage(chatId, text.ToString(), parseMode: ParseMode.Html);
			await _mainMenuHandler.ShowMainMenu(userDto, chatId);
		}

		public string GetStatusName(ComplaintStatus status) => status switch
		{
			ComplaintStatus.New => "Новая",
			ComplaintStatus.Accepted => "Принята",
			ComplaintStatus.InProgress => "В работе",
			ComplaintStatus.Resolved => "Решена",
			ComplaintStatus.Closed => "Закрыта",
			ComplaintStatus.Rejected => "Отклонена",
			_ => status.ToString()
		};
	}
}
