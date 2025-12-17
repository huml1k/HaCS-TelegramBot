using HaCSBot.Contracts.DTOs;
using HaCSBot.DataBase.Enums;
using HaCSBot.DataBase.Models;
using HaCSBot.Services.Enums;
using HaCSBot.Services.Services;
using HaCSBot.Services.Services.Extensions;
using Microsoft.OpenApi.Extensions;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static System.Net.Mime.MediaTypeNames;

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
		private readonly IUserService _userService;
		private readonly IComplaintService _complaintService;
		private readonly MainMenuHandler _mainMenuHandler;
		private readonly ILogger<UpdateHandler> _logger;

		public ComplaintHandler(
			IApartmentService apartmentService,
			ITelegramBotClient bot,
			IUserStateService userState,
			IComplaintService complaintService,
			MainMenuHandler mainMenuHandler,
			ILogger<UpdateHandler> logger,
			IUserService userService
			)
		{
			_apartmentService = apartmentService;
			_bot = bot;
			_userState = userState;
			_complaintService = complaintService;
			_mainMenuHandler = mainMenuHandler;
			_logger = logger;
			_userService = userService;
		}

		public async Task HandleReportProblem(Message message)
		{
			long chatId = message.Chat.Id;
			long userId = message.From!.Id;

			var user = await _userService.GetUserDtoAsync(userId);
			var userProfileDto = await _userService.GetProfileAsync(userId);

			if (user == null) return;

			// Получаем все квартиры пользователя
			var apartments = await _apartmentService.GetByUserIdAsync(user.Id);

			if (!apartments.Any())
			{
				await _bot.SendMessage(chatId, "У вас нет зарегистрированных квартир. Обратитесь к администратору.");
				await _mainMenuHandler.ShowMainMenu(userProfileDto, chatId);
				return;
			}

			// Если одна квартира — сразу переходим к категории
			if (apartments.Count == 1)
			{
				_userState.SetTempComplaintData(userId, new ComplaintTempDto
				{
					SelectedApartmentId = apartments.First().Id
				});
				await AskComplaintCategory(chatId, userId);
				return;
			}

			// Если несколько — просим выбрать
			var keyboardButtons = apartments.Select(a =>
				new KeyboardButton($"{a.Number} — {a.BuildingAddress}")
			).ToArray();

			var keyboard = new ReplyKeyboardMarkup(keyboardButtons)
			{
				ResizeKeyboard = true,
				OneTimeKeyboard = true
			};

			await _bot.SendMessage(chatId, "Выберите квартиру, по которой хотите сообщить о проблеме:", replyMarkup: keyboard);
			_userState.SetState(userId, ConversationState.AwaitingComplaintApartment);
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
			tempData.Apartments = apartments;
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

			var selectedApartment = await _apartmentService.GetByIdAsync(tempData.SelectedApartmentId.Value);

			string apartmentText = selectedApartment != null
				? $"{selectedApartment.Number} — {selectedApartment.BuildingAddress}"
				: "Неизвестная квартира";

			var keyboard = new ReplyKeyboardMarkup(new[]
			{
				new KeyboardButton("Отправить")
			})
			{
				ResizeKeyboard = true,
				OneTimeKeyboard = true
			};


			await _bot.SendMessage(chatId,
				$"Проверьте вашу жалобу:\n\n" +
				$"🏠 Квартира: {apartmentText}\n" +
				$"📂 Категория: {tempData.SelectedCategory.GetName()}\n" + 
				$"✍️ Описание: {description}",
				replyMarkup: keyboard);

			_userState.SetState(userId, ConversationState.AwaitingComplaintPhoto);
		}

		//метод костыль обходит отправку фото
		public async Task HandleComplaintSendAnswer(Message msg, UserProfileDto userDto)
		{
			long chatId = msg.Chat.Id;
			long userId = msg.From!.Id;
			string? text = msg.Text;

			var tempData = _userState.GetTempComplaintData(userId);
			if (tempData == null) return;

			if (text == "Отправить")
			{
				await SaveComplaint(tempData, userId, chatId, userDto);
				return;
			}
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
				var complaint = await _complaintService.CreateComplaintAsync(dto, userId);

				await _bot.SendMessage(chatId,
					"✅ Жалоба успешно отправлена!\n\n" +
					"Мы приняли её в работу. Номер заявки: <b>#" + complaint.Id.ToString("N").Substring(0, 8) + " </b>\n" +
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
				text.AppendLine($"{c.Category.GetName()}");
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

        public async Task ShowAllComplaints(Message msg, UserProfileDto userDto)
        {
            long chatId = msg.Chat.Id;

            var complaints = await _complaintService.GetAllComplaintsForAdminAsync(userDto.Id);

            var text = new StringBuilder("Все жалобы:\n\n");
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

        public async Task ShowAdminComplaintsManagement(Message msg, UserProfileDto admin)
        {
            long chatId = msg.Chat.Id;
            long userId = msg.From!.Id;

            var allComplaints = await _complaintService.GetAllComplaintsForAdminAsync(admin.Id);

            // Фильтруем только активные жалобы
            var activeComplaints = allComplaints
                .Where(c => c.Status == ComplaintStatus.New ||
                            c.Status == ComplaintStatus.Accepted ||
                            c.Status == ComplaintStatus.InProgress)
                .ToList();

            if (!activeComplaints.Any())
            {
                await _bot.SendMessage(chatId, "Активных жалоб (в работе) нет.");
                await _mainMenuHandler.ShowMainMenu(admin, chatId);
                return;
            }

            var buttons = activeComplaints.Select(c =>
            {
                string status = c.Status switch
                {
                    ComplaintStatus.New => "🆕",
                    ComplaintStatus.Accepted => "⏳",
                    ComplaintStatus.InProgress => "🔧",
                    _ => "❓"
                };

                string shortId = c.Id.ToString("N").Substring(0, 8);
                return new KeyboardButton($"{status} #{shortId}");
            }).ToList();

            buttons.Add(new KeyboardButton("⬅ Назад в панель"));

            var keyboard = new ReplyKeyboardMarkup(buttons.Chunk(1))
            {
                ResizeKeyboard = true
            };

            await _bot.SendMessage(chatId, "📋 <b>Управление жалобами</b>\n\nАктивные жалобы (требуют внимания):",
                parseMode: ParseMode.Html, replyMarkup: keyboard);

            _userState.SetState(userId, ConversationState.AdminViewComplaintsList);
        }

        public async Task HandleAdminComplaintSelection(Message msg, UserProfileDto admin)
        {
            long chatId = msg.Chat.Id;
            long userId = msg.From!.Id;
            string? text = msg.Text?.Trim();

            if (text == "⬅ Назад в панель")
            {
                _userState.SetState(userId, ConversationState.None);
                await _mainMenuHandler.ShowMainMenu(admin, chatId);
                return;
            }

            // Извлекаем короткий ID из текста кнопки (например, "🆕 #a1b2c3d4")
            var parts = text.Split('#');
            if (parts.Length < 2)
            {
                await _bot.SendMessage(chatId, "Не удалось определить жалобу.");
                return;
            }

            string shortId = parts[1].Trim().Substring(0, 8);

            var complaints = await _complaintService.GetAllComplaintsForAdminAsync(admin.Id);
            var selectedComplaint = complaints.FirstOrDefault(c =>
                c.Id.ToString("N").StartsWith(shortId));

            if (selectedComplaint == null)
            {
                await _bot.SendMessage(chatId, "Жалоба не найдена.");
                _userState.SetState(userId, ConversationState.None);  
                await _mainMenuHandler.ShowMainMenu(admin, chatId);
                return;
            }

            // Теперь показываем детали и кнопки смены статуса
            await ShowComplaintDetailsAndStatusButtons(chatId, selectedComplaint, admin, userId);
        }

        private async Task ShowComplaintDetailsAndStatusButtons(long chatId, ComplaintDto complaint, UserProfileDto admin, long userId)
        {
            var details = await _complaintService.GetComplaintDetailsAsync(complaint.Id, userId);

            var sb = new StringBuilder();
            sb.AppendLine($"<b>Жалоба #{complaint.Id.ToString("N").Substring(0, 8)}</b>\n");
            sb.AppendLine($"🏠 Квартира: {details.ApartmentNumber}, {details.BuildingAddress}");
            sb.AppendLine($"📂 Категория: {details.Category}");
            sb.AppendLine($"📅 Дата: {details.CreatedDate:dd.MM.yyyy HH:mm}");
            sb.AppendLine($"📋 Статус: <b>{GetStatusName(details.Status)}</b>\n");
            sb.AppendLine($"<i>{details.Description}</i>");

            if (details.Attachments.Any())
            {
                sb.AppendLine("\n📎 Вложения:");
                foreach (var att in details.Attachments)
                {
                    await SendAttachment(chatId, att);
                }
            }

            var keyboard = new ReplyKeyboardMarkup(new[]
            {
				new[] { new KeyboardButton("Принята"), new KeyboardButton("В работе") },
				new[] { new KeyboardButton("Решена"), new KeyboardButton("Закрыта") },
				new[] { new KeyboardButton("Отклонена") },
				new[] { new KeyboardButton("⬅ Назад к списку") }
			})
            {
                ResizeKeyboard = true
            };

            await _bot.SendMessage(chatId, sb.ToString(), parseMode: ParseMode.Html, replyMarkup: keyboard);

            // Сохраняем ID выбранной жалобы
            var tempData = new ComplaintTempDto { SelectedApartmentId = complaint.Id };
            _userState.SetTempComplaintData(userId, tempData);

            _userState.SetState(userId, ConversationState.AdminChangeComplaintStatus);
        }

        public async Task HandleAdminComplaintStatusChange(Message msg, UserProfileDto admin)
        {
            long chatId = msg.Chat.Id;
            long userId = msg.From!.Id;
            string? text = msg.Text;

            if (text == "⬅ Назад к списку")
            {
                _userState.SetState(userId, ConversationState.None);
                await ShowAdminComplaintsManagement(msg, admin);
                return;
            }

            var tempData = _userState.GetTempComplaintData(userId);
            if (tempData?.SelectedApartmentId == null)
            {
                await _bot.SendMessage(chatId, "Ошибка: жалоба не выбрана.");
                return;
            }

            ComplaintStatus newStatus = text switch
            {
                "Принята" => ComplaintStatus.Accepted,
                "В работе" => ComplaintStatus.InProgress,
                "Решена" => ComplaintStatus.Resolved,
                "Закрыта" => ComplaintStatus.Closed,
                "Отклонена" => ComplaintStatus.Rejected,
                _ => ComplaintStatus.New
            };

            var dto = new ComplaintStatusChangeDto
            {
                ComplaintId = tempData.SelectedApartmentId.Value,
                Status = newStatus
            };

            try
            {
                await _complaintService.ChangeComplaintStatusAsync(dto, admin.Id);

                var complaint = await _complaintService.GetComplaintDetailsAsync(dto.ComplaintId, userId);

                // Уведомляем жильца
                var details = await _complaintService.GetComplaintDetailsAsync(dto.ComplaintId, userId);
                if (details?.ApartmentId != null)
                {
                    var apartment = await _apartmentService.GetByUserIdAsync(details.ApartmentId);
                    if (apartment != null)
                    {
                        await _bot.SendMessage(userId,
                            $"🔧 Статус вашей жалобы #{dto.ComplaintId.ToString("N").Substring(0, 8)} изменён\n\n" +
                            $"Новый статус: <b>{text}</b>",
                            parseMode: ParseMode.Html);
                    }
                }

                await _bot.SendMessage(chatId, $"Статус изменён на: <b>{text}</b>", parseMode: ParseMode.Html);
            }
            catch
            {
                await _bot.SendMessage(chatId, "Ошибка при изменении статуса.");
            }

            _userState.ClearTempComplaintData(userId);
            _userState.SetState(userId, ConversationState.None);
            await ShowAdminComplaintsManagement(msg, admin);
        }

        private async Task SendAttachment(long chatId, AttachmentDto attachment)
        {
            try
            {
                if (attachment.Type == AttachmentType.Photo)
                    await _bot.SendPhoto(chatId, attachment.TelegramFileId, caption: attachment.Caption);
                else if (attachment.Type == AttachmentType.Document)
                    await _bot.SendDocument(chatId, attachment.TelegramFileId, caption: attachment.Caption);
            }
            catch
            {
                await _bot.SendMessage(chatId, "Не удалось отправить вложение.");
            }
        }
    }
}
