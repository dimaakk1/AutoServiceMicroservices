using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoserviceTelegram.BLL.Settings
{
    public class TelegramSettings
    {
        public string BotToken { get; set; } = "";
        public long AdminChatId { get; set; }
    }
}
