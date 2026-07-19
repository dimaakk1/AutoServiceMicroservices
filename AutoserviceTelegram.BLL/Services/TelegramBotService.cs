using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace AutoserviceTelegram.BLL.Services
{
    public class TelegramBotService : ITelegramBotService
    {
        private readonly TelegramBotClient _bot;
        private readonly IConfiguration _configuration;

        public TelegramBotService(IConfiguration configuration)
        {
            _configuration = configuration;

            var token =
              Environment.GetEnvironmentVariable("BotToken");

            _bot = new TelegramBotClient(token);
        }

        
        public async Task SendMessageAsync(
    string message,
    InlineKeyboardMarkup? keyboard = null)
        {
            long chatId =
                long.Parse(
                    Environment.GetEnvironmentVariable("AdminChatId")!);
            await _bot.SendMessage(
                chatId: chatId,
                text: message,
                replyMarkup: keyboard
            );

        }
    }
}
