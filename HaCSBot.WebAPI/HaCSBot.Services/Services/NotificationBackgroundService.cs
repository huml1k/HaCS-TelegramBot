using HaCSBot.Services.Services.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HaCSBot.Services.Services
{
    public class NotificationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NotificationBackgroundService> _logger;
        private readonly PeriodicTimer _timer;

        public NotificationBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<NotificationBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _timer = new PeriodicTimer(TimeSpan.FromMinutes(1)); // Проверяем каждую минуту
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Notification Background Service запущен");

            while (await _timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                        await notificationService.SendScheduledNotificationsAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка в фоновой службе уведомлений");
                }
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Notification Background Service остановлен");
            await base.StopAsync(cancellationToken);
        }
    }
}
