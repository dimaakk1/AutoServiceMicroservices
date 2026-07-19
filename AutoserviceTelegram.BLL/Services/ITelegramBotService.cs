using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace AutoserviceTelegram.BLL.Services
{
    public interface ITelegramBotService
    {
        Task SendMessageAsync(string message);
    }
}
