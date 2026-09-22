using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using static System.Formats.Asn1.AsnWriter;

namespace AutoserviceTelegram.BLL.Services
{
    public class TelegramCommandHandler
    {
        private readonly IOrderGrpcService _orders;
        private readonly UserService.UserServiceClient _userClient;


        public TelegramCommandHandler(
            IOrderGrpcService orders,
            UserService.UserServiceClient userClient)
        {
            _orders = orders;
            _userClient = userClient;
        }


        private async Task SendMainMenu(
    ITelegramBotClient bot,
    Message message,
    CancellationToken token)
        {
            var keyboard = new ReplyKeyboardMarkup(
                new[]
                {
            new KeyboardButton[]
            {
                "📅 Сьогодні",
                "📅 Завтра"
            },

            new KeyboardButton[]
            {
                "⏳ Очікують",
                "✅ Підтверджені"
            },

            new KeyboardButton[]
            {
                "🔧 В роботі",
                "🏁 Завершені"
            },

            new KeyboardButton[]
            {
                "❌ Скасовані",
                "📊 Статистика"
            }
                })
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = false
            };


            await bot.SendMessage(
                message.Chat.Id,
                """
        🚗 AutoService Admin

        Виберіть дію:
        """,
                replyMarkup: keyboard,
                cancellationToken: token);
        }


        public async Task HandleAsync(
            ITelegramBotClient bot,
            Update update,
            CancellationToken token)
        {

            if (update.CallbackQuery != null)
            {
                await HandleCallback(
                    bot,
                    update.CallbackQuery,
                    token);

                return;
            }


            if (update.Message != null)
            {
                await HandleMessage(
                    bot,
                    update.Message,
                    token);
            }
        }





        private async Task HandleMessage(
            ITelegramBotClient bot,
            Message message,
            CancellationToken token)
        {

            var text = message.Text;


            if (string.IsNullOrEmpty(text))
                return;



            switch (text)
            {

                case "📅 Сьогодні":

                case "/today":

                    await SendOrders(
                        bot,
                        message,
                        await _orders.GetTodayOrdersAsync(),
                        token);

                    break;



                case "📅 Завтра":

                case "/tomorrow":

                    await SendOrders(
                        bot,
                        message,
                        await _orders.GetTomorrowOrdersAsync(),
                        token);

                    break;



                case "⏳ Очікують":

                case "/pending":

                    await SendOrders(
                        bot,
                        message,
                        await _orders.GetOrdersByStatusAsync("Pending"),
                        token);

                    break;



                case "✅ Підтверджені":

                case "/confirmed":

                    await SendOrders(
                        bot,
                        message,
                        await _orders.GetOrdersByStatusAsync("Confirmed"),
                        token);

                    break;



                case "🔧 В роботі":

                case "/progress":

                    await SendOrders(
                        bot,
                        message,
                        await _orders.GetOrdersByStatusAsync("InProgress"),
                        token);

                    break;



                case "🏁 Завершені":

                case "/completed":

                    await SendOrders(
                        bot,
                        message,
                        await _orders.GetOrdersByStatusAsync("Completed"),
                        token);

                    break;



                case "❌ Скасовані":

                case "/cancelled":

                    await SendOrders(
                        bot,
                        message,
                        await _orders.GetOrdersByStatusAsync("Cancelled"),
                        token);

                    break;



     



                case "/start":

                    await SendMainMenu(
                        bot,
                        message,
                        token);

                    break;



                default:

                    await bot.SendMessage(
                        message.Chat.Id,
                        "❌ Невідома команда",
                        cancellationToken: token);

                    break;

            }
        }

        







        private async Task SendOrders(
            ITelegramBotClient bot,
            Message message,
            OrderListResponse orders,
            CancellationToken token)
        {

            if (orders.Orders.Count == 0)
            {

                await bot.SendMessage(
                    message.Chat.Id,
                    "Замовлень немає",
                    cancellationToken: token);

                return;
            }



            foreach (var order in orders.Orders)
            {


                var user =
                    await _userClient.GetUserAsync(
                        new UserRequest
                        {
                            UserId = order.UserId
                        });



                var keyboard =
                    new InlineKeyboardMarkup(
                    new[]
                    {

                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData(
                                "✅ Підтвердити",
                                $"status:{order.OrderId}:Confirmed"),


                            InlineKeyboardButton.WithCallbackData(
                                "🔧 В роботу",
                                $"status:{order.OrderId}:InProgress")
                        },


                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData(
                                "🏁 Завершити",
                                $"status:{order.OrderId}:Completed"),


                            InlineKeyboardButton.WithCallbackData(
                                "❌ Скасувати",
                                $"status:{order.OrderId}:Cancelled")
                        }

                    });




                await bot.SendMessage(
                    message.Chat.Id,

$"""
🚗 Замовлення #{order.OrderId}


👤 Клієнт:
{user.Username}


📌 Статус:
{order.Status}


📅 Дата:
{DateTime.Parse(order.OrderDate):dd.MM.yyyy}


⏰ Час:
{DateTime.Parse(order.OrderDate):HH:mm}


""",
                    replyMarkup: keyboard,
                    cancellationToken: token);

            }

        }








        private async Task HandleCallback(
            ITelegramBotClient bot,
            CallbackQuery callback,
            CancellationToken token)
        {


            if (string.IsNullOrEmpty(callback.Data))
                return;



            var parts =
                callback.Data.Split(":");



            if (parts.Length != 3)
                return;



            if (parts[0] != "status")
                return;




            int orderId =
                int.Parse(parts[1]);



            string status =
                parts[2];



            var result =
                await _orders.UpdateStatusAsync(
                    orderId,
                    status);



            if (result)
            {

                await bot.AnswerCallbackQuery(
                    callback.Id,
                    "Статус змінено",
                    cancellationToken: token);



                await bot.EditMessageText(
                    callback.Message!.Chat.Id,
                    callback.Message.MessageId,
$"""
🚗 Замовлення #{orderId}

✅ Новий статус:
{status}
""",
                    cancellationToken: token);

            }

        }

    }
}
