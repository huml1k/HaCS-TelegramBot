namespace HaCSBot.WebAPI
{
    public class BotConfiguration
    {
        public string BotToken { get; init; } = default!;
        public Uri BotWebhookUrl { get; init; } = default!;
    }
}
