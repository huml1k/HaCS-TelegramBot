using HaCSBot.DataBase.Enums;
using HaCSBot.Services.Enums;
using HaCSBot.Services.Services.Extensions;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace HaCSBot.WebAPI.Handlers
{
	public class UpdateHandler : IUpdateHandler
	{
        private readonly MessageHandler _massageHandler;
        private readonly CallbackQueryHandler _callbackQueryHandler;
		private readonly ILogger<UpdateHandler> _logger;

		public UpdateHandler(
			MessageHandler massageHandler,
			CallbackQueryHandler callbackQueryHandler,
			ILogger<UpdateHandler> logger)

		{
            _massageHandler = massageHandler;
            _callbackQueryHandler = callbackQueryHandler;
            _logger = logger;
        }

        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            _logger.LogInformation("HandleError: {Exception}", exception);
            if (exception is RequestException)
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await (update switch
            {
                { Message: { } message } => _massageHandler.HandleMessageAsync(message),
                { EditedMessage: { } message } => _massageHandler.HandleMessageAsync(message),
                { CallbackQuery: { } callbackQuery } => _callbackQueryHandler.OnCallbackQuery(callbackQuery),
                _ => UnknownUpdateHandlerAsync(update)
            });
        }

		private Task UnknownUpdateHandlerAsync(Update update)
		{
			_logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
			return Task.CompletedTask;
		}

    }
}
