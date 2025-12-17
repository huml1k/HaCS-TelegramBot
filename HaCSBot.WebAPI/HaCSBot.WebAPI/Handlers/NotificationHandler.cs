using HaCSBot.Contracts.DTOs;
using HaCSBot.DataBase.Enums;
using HaCSBot.Services.Enums;
using HaCSBot.Services.Services.Extensions;
using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace HaCSBot.WebAPI.Handlers
{
    public class NotificationHandler
    {
        private readonly ITelegramBotClient _bot;
        private readonly IUserStateService _userState;
        private readonly INotificationService _notificationService;
        private readonly IBuildingService _buildingService;
        private readonly MainMenuHandler _mainMenuHandler;
        private readonly AdminPanelHandler _adminPanelHandler;
        private readonly ILogger<NotificationHandler> _logger;

        public NotificationHandler(
            ITelegramBotClient bot,
            IUserStateService userState,
            INotificationService notificationService,
            IBuildingService buildingService,
            MainMenuHandler mainMenuHandler,
            AdminPanelHandler adminPanelHandler,
            ILogger<NotificationHandler> logger)
        {
            _bot = bot;
            _userState = userState;
            _notificationService = notificationService;
            _buildingService = buildingService;
            _mainMenuHandler = mainMenuHandler;
            _adminPanelHandler = adminPanelHandler;
            _logger = logger;
        }

        public async Task HandleCreateNotificationStart(Message msg, UserProfileDto admin)
        {
            long chatId = msg.Chat.Id;
            long userId = msg.From!.Id;

            _userState.ClearTempNotificationData(userId);

            var tempData = new NotificationTempDto
            {
                CreatorId = admin.Id,
                CurrentStep = 1
            };

            _userState.SetTempNotificationData(userId, tempData);

            var keyboard = new ReplyKeyboardMarkup(new[]
            {
                new[] { new KeyboardButton("📢 Общее (всем)"), new KeyboardButton("🏠 По дому") },
                new[] { new KeyboardButton("⬅ Назад в панель") }
            })
            {
                ResizeKeyboard = true
            };

            await _bot.SendMessage(
                chatId,
                "Кому отправить уведомление?",
                replyMarkup: keyboard
            );

            _userState.SetState(userId, ConversationState.AdminNotificationRecipient);
        }

        public async Task HandleRecipientSelection(Message msg, UserProfileDto admin)
        {
            long chatId = msg.Chat.Id;
            long userId = msg.From!.Id;
            string? text = msg.Text?.Trim();

            if (text == "⬅ Назад в панель")
            {
                _userState.SetState(userId, ConversationState.None);
                await _adminPanelHandler.ShowAdminPanel(chatId);
                return;
            }

            var tempData = _userState.GetTempNotificationData(userId);
            if (tempData == null)
            {
                await HandleCreateNotificationStart(msg, admin);
                return;
            }

            switch (text)
            {
                case "📢 Общее (всем)":
                    tempData.BuildingId = null;
                    _userState.SetTempNotificationData(userId, tempData);
                    await AskNotificationType(chatId, userId);
                    return;

                case "🏠 По дому":
                    var buildings = await _buildingService.GetAllBuildingsAsync();

                    if (!buildings.Any())
                    {
                        await _bot.SendMessage(
                            chatId,
                            "В системе нет зарегистрированных домов."
                        );
                        await _adminPanelHandler.ShowAdminPanel(chatId);
                        return;
                    }

                    var buildingButtons = buildings
                        .Select(b => new KeyboardButton(b.StreetName))
                        .ToList();

                    buildingButtons.Add(new KeyboardButton("⬅ Назад"));

                    var buttonRows = buildingButtons
                        .Select((btn, index) => new { btn, index })
                        .GroupBy(x => x.index / 2)
                        .Select(group => group.Select(x => x.btn).ToArray())
                        .ToList();

                    var buildingKeyboard = new ReplyKeyboardMarkup(buttonRows)
                    {
                        ResizeKeyboard = true
                    };

                    await _bot.SendMessage(
                        chatId,
                        "Выберите дом:",
                        replyMarkup: buildingKeyboard
                    );

                    _userState.SetState(userId, ConversationState.AdminNotificationBuilding);
                    return;

                default:
                    await HandleCreateNotificationStart(msg, admin);
                    return;
            }
        }

        public async Task HandleBuildingSelection(Message msg, UserProfileDto admin)
        {
            long chatId = msg.Chat.Id;
            long userId = msg.From!.Id;
            string? text = msg.Text?.Trim();

            if (text == "⬅ Назад")
            {
                await HandleCreateNotificationStart(msg, admin);
                return;
            }

            var buildings = await _buildingService.GetAllBuildingsAsync();
            var selected = buildings.FirstOrDefault(b => b.StreetName == text);

            if (selected == null)
            {
                await _bot.SendMessage(chatId, "Дом не найден. Выберите из списка.");
                return;
            }

            var tempData = _userState.GetTempNotificationData(userId);
            if (tempData == null)
            {
                await HandleCreateNotificationStart(msg, admin);
                return;
            }

            tempData.BuildingId = selected.Id;
            _userState.SetTempNotificationData(userId, tempData);

            await AskNotificationType(chatId, userId);
        }

        private async Task AskNotificationType(long chatId, long userId)
        {
            var keyboard = new ReplyKeyboardMarkup(new[]
            {
                new[] { new KeyboardButton("🛠 Плановые работы"), new KeyboardButton("🚨 Аварийное отключение") },
                new[] { new KeyboardButton("💧 Отключение ресурсов"), new KeyboardButton("📢 Общее объявление") },
                new[] { new KeyboardButton("💰 Изменение тарифов"), new KeyboardButton("💸 Напоминание об оплате") },
                new[] { new KeyboardButton("📊 Напоминание показаний"), new KeyboardButton("🏠 Собрание жильцов") },
                new[] { new KeyboardButton("✅ Статус заявки"), new KeyboardButton("📌 Прочие") },
                new[] { new KeyboardButton("⬅ Назад") }
            })
            {
                ResizeKeyboard = true
            };

            await _bot.SendMessage(
                chatId,
                "Выберите тип уведомления:",
                replyMarkup: keyboard
            );

            _userState.SetState(userId, ConversationState.AdminNotificationType);
        }

        public async Task HandleNotificationType(Message msg, UserProfileDto admin)
        {
            long chatId = msg.Chat.Id;
            long userId = msg.From!.Id;
            string? text = msg.Text?.Trim();

            if (text == "⬅ Назад")
            {
                await HandleCreateNotificationStart(msg, admin);
                return;
            }

            var tempData = _userState.GetTempNotificationData(userId);
            if (tempData == null)
            {
                await HandleCreateNotificationStart(msg, admin);
                return;
            }

            tempData.Type = MapNotificationType(text);
            _userState.SetTempNotificationData(userId, tempData);

            await _bot.SendMessage(chatId, "Введите заголовок уведомления:");
            _userState.SetState(userId, ConversationState.AdminNotificationTitle);
        }

        private NotificationType MapNotificationType(string text)
        {
            return text switch
            {
                "🛠 Плановые работы" => NotificationType.PlannedMaintenance,
                "🚨 Аварийное отключение" => NotificationType.EmergencyShutdown,
                "💧 Отключение ресурсов" => NotificationType.ResourceShutdown,
                "📢 Общее объявление" => NotificationType.GeneralAnnouncement,
                "💰 Изменение тарифов" => NotificationType.TariffChange,
                "💸 Напоминание об оплате" => NotificationType.PaymentReminder,
                "📊 Напоминание показаний" => NotificationType.MeterReadingReminder,
                "🏠 Собрание жильцов" => NotificationType.MeetingAnnouncement,
                "✅ Статус заявки" => NotificationType.ComplaintStatusUpdate,
                "📌 Прочие" => NotificationType.Other,
                _ => NotificationType.Other
            };
        }

        public async Task HandleNotificationTitle(Message msg, UserProfileDto admin)
        {
            long chatId = msg.Chat.Id;
            long userId = msg.From!.Id;
            string? text = msg.Text?.Trim();

            if (text == "⬅ Назад")
            {
                await AskNotificationType(chatId, userId);
                return;
            }

            var tempData = _userState.GetTempNotificationData(userId);
            if (tempData == null)
            {
                await HandleCreateNotificationStart(msg, admin);
                return;
            }

            tempData.Title = text;
            _userState.SetTempNotificationData(userId, tempData);

            await _bot.SendMessage(chatId, "Введите текст уведомления:");
            _userState.SetState(userId, ConversationState.AdminNotificationMessage);
        }

        public async Task HandleNotificationMessage(Message msg, UserProfileDto admin)
        {
            long chatId = msg.Chat.Id;
            long userId = msg.From!.Id;
            string? text = msg.Text?.Trim();

            if (text == "⬅ Назад")
            {
                await _bot.SendMessage(chatId, "Введите заголовок уведомления:");
                _userState.SetState(userId, ConversationState.AdminNotificationTitle);
                return;
            }

            var tempData = _userState.GetTempNotificationData(userId);
            if (tempData == null)
            {
                await HandleCreateNotificationStart(msg, admin);
                return;
            }

            tempData.Message = text;
            _userState.SetTempNotificationData(userId, tempData);

            var keyboard = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton("Отправить сейчас"),
                new KeyboardButton("Запланировать отправку"),
                new KeyboardButton("⬅ Назад")
            })
            {
                ResizeKeyboard = true
            };

            await _bot.SendMessage(chatId, "Выберите время отправки:", replyMarkup: keyboard);
            _userState.SetState(userId, ConversationState.AdminNotificationScheduled);
        }

        public async Task HandleNotificationScheduledOrAttachments(Message msg, UserProfileDto admin)
        {
            long chatId = msg.Chat.Id;
            long userId = msg.From!.Id;
            string? text = msg.Text?.Trim();

            if (text == "⬅ Назад")
            {
                await _bot.SendMessage(chatId, "Введите текст уведомления:");
                _userState.SetState(userId, ConversationState.AdminNotificationMessage);
                return;
            }

            var tempData = _userState.GetTempNotificationData(userId);
            if (tempData == null)
            {
                await HandleCreateNotificationStart(msg, admin);
                return;
            }

            if (text == "Запланировать отправку")
            {
                await _bot.SendMessage(chatId, "Введите дату и время отправки (формат: dd.MM.yyyy HH:mm) в UTC:");
                _userState.SetState(userId, ConversationState.AdminNotificationScheduledDate);
                return;
            }

            tempData.ScheduledSendDate = null;
            _userState.SetTempNotificationData(userId, tempData);

            await CreateAndSendNotification(tempData, chatId, admin, userId);
        }

        public async Task HandleNotificationScheduledDate(Message msg, UserProfileDto admin)
        {
            long chatId = msg.Chat.Id;
            long userId = msg.From!.Id;
            string? text = msg.Text?.Trim();

            if (text == "⬅ Назад")
            {
                var keyboard = new ReplyKeyboardMarkup(new[]
                {
                    new KeyboardButton("Отправить сейчас"),
                    new KeyboardButton("Запланировать отправку"),
                    new KeyboardButton("⬅ Назад")
                })
                {
                    ResizeKeyboard = true
                };

                await _bot.SendMessage(chatId, "Выберите время отправки:", replyMarkup: keyboard);
                _userState.SetState(userId, ConversationState.AdminNotificationScheduled);
                return;
            }

            var tempData = _userState.GetTempNotificationData(userId);
            if (tempData == null)
            {
                await HandleCreateNotificationStart(msg, admin);
                return;
            }

            if (DateTime.TryParseExact(text, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var scheduledDate))
            {
                if (scheduledDate.Kind != DateTimeKind.Utc)
                {
                    scheduledDate = DateTime.SpecifyKind(scheduledDate, DateTimeKind.Utc);
                }

                if (scheduledDate < DateTime.UtcNow.AddMinutes(-1))
                {
                    await _bot.SendMessage(chatId, "Дата должна быть в будущем. Введите корректную дату:");
                    return;
                }

                tempData.ScheduledSendDate = scheduledDate;
                _userState.SetTempNotificationData(userId, tempData);

                await CreateAndSendNotification(tempData, chatId, admin, userId);
            }
            else
            {
                await _bot.SendMessage(chatId, "Неверный формат даты. Введите дату в формате dd.MM.yyyy HH:mm:");
            }
        }

        private async Task CreateAndSendNotification(NotificationTempDto tempData, long chatId, UserProfileDto admin, long telegramUserId)
        {
            try
            {
                if (!tempData.BuildingId.HasValue)
                {
                    var buildings = await _buildingService.GetAllBuildingsAsync();
                    _logger.LogInformation($"Найдено {buildings.Count} домов для создания уведомлений");

                    int successCount = 0;

                    foreach (var building in buildings)
                    {
                        try
                        {
                            var dto = new CreateNotificationDto
                            {
                                Type = tempData.Type,
                                Title = tempData.Title!,
                                Message = tempData.Message!,
                                BuildingId = building.Id, 
                                ScheduledSendDate = tempData.ScheduledSendDate,
                                Attachments = tempData.Attachments
                            };

                            var notificationId = await _notificationService.CreateNotificationAsync(dto, tempData.CreatorId);
                            _logger.LogInformation($"Создано уведомление ID: {notificationId} для дома: {building.StreetName}");
                            successCount++;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Ошибка при создании уведомления для дома {BuildingAddress}", building.StreetName);
                        }
                    }

                    string statusText = tempData.ScheduledSendDate.HasValue
                        ? $"Запланировано на {tempData.ScheduledSendDate.Value:dd.MM.yyyy HH:mm} UTC"
                        : "Отправляется";

                    await _bot.SendMessage(chatId,
                        $"✅ Создано {successCount}/{buildings.Count} уведомлений для всех домов!\n{statusText}");
                }
                // Если указан конкретный дом
                else
                {
                    var dto = new CreateNotificationDto
                    {
                        Type = tempData.Type,
                        Title = tempData.Title!,
                        Message = tempData.Message!,
                        BuildingId = tempData.BuildingId,
                        ScheduledSendDate = tempData.ScheduledSendDate,
                        Attachments = tempData.Attachments
                    };

                    var notificationId = await _notificationService.CreateNotificationAsync(dto, tempData.CreatorId);

                    _logger.LogInformation($"Создано уведомление ID: {notificationId} для дома ID: {tempData.BuildingId}");

                    string statusText = tempData.ScheduledSendDate.HasValue
                        ? $"Запланировано на {tempData.ScheduledSendDate.Value:dd.MM.yyyy HH:mm} UTC"
                        : "Отправляется сейчас";

                    await _bot.SendMessage(chatId, $"✅ Уведомление создано!\n{statusText}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка создания уведомления");
                await _bot.SendMessage(chatId, $"❌ Ошибка при создании уведомления: {ex.Message}");
            }
            finally
            {
                _userState.ClearTempNotificationData(telegramUserId);
                _userState.SetState(telegramUserId, ConversationState.None);
                await _mainMenuHandler.ShowMainMenu(admin, chatId);
            }
        }
    }
}