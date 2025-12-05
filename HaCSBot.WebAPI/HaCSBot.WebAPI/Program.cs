
using HaCSBot.WebAPI.Handlers;
using Microsoft.AspNetCore;
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
            builder.Services.AddSingleton<UpdateHandler>();

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
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
