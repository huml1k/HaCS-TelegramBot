using HaCSBot.DataBase;
using HaCSBot.DataBase.Repositories;
using HaCSBot.DataBase.Repositories.Extensions;
using HaCSBot.Services.Services;
using HaCSBot.Services.Services.Extensions;
using HaCSBot.WebAPI.Handlers;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;

namespace HaCSBot.WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var botConfigSection = builder.Configuration.GetSection("BotConfiguration");
            builder.Services.Configure<BotConfiguration>(botConfigSection);
            builder.Services.AddHttpClient("tgwebhook").RemoveAllLoggers().AddTypedClient<ITelegramBotClient>(
                httpClient => new TelegramBotClient(botConfigSection.Get<BotConfiguration>()!.BotToken, httpClient));

			builder.Services.AddDbContext<MyApplicationDbContext>(options =>
				options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            //builder.Services.AddStackExchangeRedisCache(options =>
            //{
            //    options.Configuration = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
            //    options.InstanceName = "HaCSBot_";
            //});

            builder.Services.AddAutoMapper(typeof(Program));

			builder.Services.AddScoped<UpdateHandler>();
			builder.Services.AddSingleton<IUserStateService, InMemoryUserStateService>();
			builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IBuildingService, BuildingService>();
            builder.Services.AddScoped<IComplaintService, ComplaintService>();
            builder.Services.AddScoped<IFileService, FileService>();
            builder.Services.AddScoped<IMeterReadingService, MeterReadingService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<ITariffService, TariffService>();
            builder.Services.AddScoped<IApartmentService, ApartmentService>();

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

            app.Run();
        }
    }
}
