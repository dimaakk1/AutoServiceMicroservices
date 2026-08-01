using AutoserviceTelegram.BLL.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace AutoserviceTelegram.BLL.Consumers
{
    public class TelegramBotWorker : BackgroundService
    {



        private readonly IServiceScopeFactory _scopeFactory;
        private readonly TelegramBotClient _bot;


        public TelegramBotWorker(
            IConfiguration config,
            IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;

            var botToken = config["TelegramSettings:BotToken"]
                ?? Environment.GetEnvironmentVariable("BotToken")
                ?? throw new InvalidOperationException("Telegram bot token is not configured.");

            _bot = new TelegramBotClient(botToken);
        }



        protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
        {

            _bot.StartReceiving(

            async (bot, update, token) =>
            {
                using var scope =
    _scopeFactory.CreateScope();


                var handler =
                    scope.ServiceProvider
                    .GetRequiredService<TelegramCommandHandler>();


                await handler.HandleAsync(
                    bot,
                    update,
                    token);
            },

            async (bot, error, token) =>
            {
                Console.WriteLine(error.Message);
            });


            Console.WriteLine(
            "Telegram Bot Started");


            await Task.Delay(
            Timeout.Infinite,
            stoppingToken);

        }

    }
}
