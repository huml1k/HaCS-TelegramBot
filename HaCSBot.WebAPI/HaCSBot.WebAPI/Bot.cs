using Telegram.Bot;

namespace HaCSBot.WebAPI
{
    public class Bot
    {
        private static TelegramBotClient client { get; set; }

        public static TelegramBotClient GetTelegramBot()
        {
            if (client != null)
            {
                return client;
            }
            client = new TelegramBotClient("Ваш токен");
            return client;
        }
    }
}
