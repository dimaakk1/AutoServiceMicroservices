using AutoserviceTelegram.BLL.Contract;
using AutoserviceTelegram.BLL.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace AutoserviceTelegram.BLL.Consumers
{
    public class OrderCreatedConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _config;

        private IConnection? _connection;
        private IChannel? _channel;

        public OrderCreatedConsumer(
            IServiceScopeFactory scopeFactory,
            IConfiguration config)
        {
            _scopeFactory = scopeFactory;
            _config = config;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            var factory = new ConnectionFactory
            {
                Uri = new Uri(_config.GetConnectionString("rabbitmq")!)
            };

            _connection = await factory.CreateConnectionAsync(cancellationToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
            Console.WriteLine("RabbitMQ Connected");
            await _channel.ExchangeDeclareAsync(
    exchange: "order-created-exchange",
    type: ExchangeType.Fanout,
    durable: true);

            await _channel.QueueDeclareAsync(
                queue: "telegram-orders",
                durable: true,
                exclusive: false,
                autoDelete: false);

            await _channel.QueueBindAsync(
                queue: "telegram-orders",
                exchange: "order-created-exchange",
                routingKey: "");

            await base.StartAsync(cancellationToken);
            Console.WriteLine("Telegram Consumer Started");
        }

        protected override Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            var consumer =
                new AsyncEventingBasicConsumer(_channel!);

            consumer.ReceivedAsync += async (_, e) =>
            {
                Console.WriteLine("MESSAGE RECEIVED");

                using var scope =
                    _scopeFactory.CreateScope();

                try
                {
                    var telegram =
                        scope.ServiceProvider
                            .GetRequiredService<ITelegramBotService>();

                    var json =
                        Encoding.UTF8.GetString(
                            e.Body.ToArray());

                    var order =
                        JsonSerializer.Deserialize<OrderCreatedEvent>(
                            json);

                    if (order == null)
                        return;

                    await telegram.SendMessageAsync(
    $"""
🚗 Нове замовлення

№ {order.OrderId}

👤 UserId:
{order.UserId}

📅 Дата:
{order.OrderDate:dd.MM.yyyy}

⏰ Час:
{order.OrderDate:HH:mm}
""");

                    await _channel!.BasicAckAsync(
                        e.DeliveryTag,
                        false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);

                    await _channel!.BasicNackAsync(
                        e.DeliveryTag,
                        false,
                        true);
                }
            };

            _channel.BasicConsumeAsync(
    queue: "telegram-orders",
    autoAck: false,
    consumer: consumer);
            Console.WriteLine("ExecuteAsync Started");
            return Task.CompletedTask;
        }
    }
}
