using HaCSBot.Services.Commands.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace HaCSBot.Services.Commands
{
    public class StartCommand : ICommand
    {
        public TelegramBotClient Client => throw new NotImplementedException();

        public string Name => "/start";

        public Task Execute(Update update)
        {
            throw new NotImplementedException();
        }
    }
}
