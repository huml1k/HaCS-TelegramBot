using HaCSBot.Contracts.AutoMapping;
using HaCSBot.DataBase;
using HaCSBot.DataBase.Repositories;
using HaCSBot.DataBase.Repositories.Extensions;
using HaCSBot.Services.Senders.Extensions;
using HaCSBot.Services.Services;
using HaCSBot.Services.Services.Extensions;
using HaCSBot.WebAPI.Handlers;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Telegram.Bot;

namespace HaCSBot.WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var botConfigSection = builder.Configuration.GetSection("BotConfiguration");

            builder.Services.Configure<BotConfiguration>(botConfigSection);

            builder.Services.AddHttpClient("tgwebhook").RemoveAllLoggers().AddTypedClient<ITelegramBotClient>(
                httpClient => new TelegramBotClient(botConfigSection.Get<BotConfiguration>()!.BotToken, httpClient));

			builder.Services.AddDbContext<MyApplicationDbContext>(options =>
				options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddHostedService<NotificationBackgroundService>();

            //builder.Services.AddStackExchangeRedisCache(options =>
            //{
            //    options.Configuration = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
            //    options.InstanceName = "HaCSBot_";
            //});

            builder.Services.AddMemoryCache();
			builder.Services.AddAutoMapper(
				typeof(UserMapping).Assembly,
				typeof(BuildingMapping).Assembly,
				typeof(ApartmentMapping).Assembly,
				typeof(ComplaintMapping).Assembly,
				typeof(FileMappingProfile).Assembly,
				typeof(NotificationMapping).Assembly,
				typeof(Program).Assembly
				);
			//handlers
			builder.Services.AddScoped<UpdateHandler>();
			builder.Services.AddScoped<CallbackQueryHandler>();
			builder.Services.AddScoped<ComplaintHandler>();
			builder.Services.AddScoped<MainMenuHandler>();
			builder.Services.AddScoped<MessageHandler>();
			builder.Services.AddScoped<MeterReadingHandler>();
			//builder.Services.AddScoped<RegistrationHandler>();
			builder.Services.AddScoped<StateDispatcherHandler>();
			builder.Services.AddScoped<TariffHandler>();
            builder.Services.AddScoped<AdminPanelHandler>();
			builder.Services.AddScoped<NotificationHandler>();
            //services
            builder.Services.AddScoped<ITelegramNotificationSender, TelegramNotificationSender>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IBuildingService, BuildingService>();
            builder.Services.AddScoped<IComplaintService, ComplaintService>();
            builder.Services.AddScoped<IFileService, FileService>();
            builder.Services.AddScoped<IMeterReadingService, MeterReadingService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<ITariffService, TariffService>();
            builder.Services.AddScoped<IApartmentService, ApartmentService>();
			builder.Services.AddSingleton<IUserStateService, InMemoryUserStateService>();
			//repositories
			builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IApartmentRepository, ApartmentRepository>();
            builder.Services.AddScoped<IBuildingMaintenanceRepository, BuildingMaintenanceRepository>();
            builder.Services.AddScoped<IBuildingRepository, BuildingRepository>();
            builder.Services.AddScoped<IComplaintAttachmentRepository, ComplaintAttachmentRepository>();
            builder.Services.AddScoped<IComplaintRepository, ComplaintRepository>();
            builder.Services.AddScoped<IMeterReadingRepository, MeterReadingRepository>();
            builder.Services.AddScoped<INotificationAttachmentRepository, NotificationAttachmentRepository>();
            builder.Services.AddScoped<INotificationDeliveryRepository, NotificationDeliveryRepository>();
            builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
            builder.Services.AddScoped<ITariffRepository, TariffRepository>();
            
            builder.Services.AddControllers();

			builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}

			app.UseHttpsRedirection();
            app.UseAuthorization();

			app.MapControllers();

			app.MapFallback(async context =>
			{
				if (context.Request.Method == "CONNECT")
				{
					context.Response.StatusCode = 200;
					return;
				}

				context.Response.StatusCode = 200;
				await context.Response.WriteAsync("Bot is running");
			});

			app.Run();
        }
    }
}
