using HaCSBot.Contracts.DTOs;
using HaCSBot.Services.Enums;
using HaCSBot.Services.Services.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace HaCSBot.WebAPI.Handlers
{
    public class AdminPanelHandler
    {
        private readonly ITelegramBotClient _bot;
        private readonly MainMenuHandler _mainMenuHandler;

        public AdminPanelHandler(
            ITelegramBotClient bot,
            MainMenuHandler mainMenuHandler)
        {
            _bot = bot;
            _mainMenuHandler = mainMenuHandler;
        }

        public async Task ShowAdminPanel(long chatId)
        {
            var keyboard = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton("Создать уведомление"),
                new KeyboardButton("Управление жалобами"),
                new KeyboardButton("Управление домами"),
                new KeyboardButton("⬅ Назад в главное меню")
            })
            {
                ResizeKeyboard = true
            };

            await _bot.SendMessage(chatId, "🔧 <b>Панель управления</b>",
                parseMode: ParseMode.Html,
                replyMarkup: keyboard);
        }
    }
}