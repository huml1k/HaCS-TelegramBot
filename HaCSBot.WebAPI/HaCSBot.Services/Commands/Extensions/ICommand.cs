using Telegram.Bot;
using Telegram.Bot.Types;

namespace HaCSBot.Services.Commands.Extensions
{
    public interface ICommand
    {
        public TelegramBotClient Client { get; }

        public string Name { get; }

        public Task Execute(Update update);
    }
}
