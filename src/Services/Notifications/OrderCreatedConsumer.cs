using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace AutoserviceNotification
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

            await _channel.ExchangeDeclareAsync(
    exchange: "order-created-exchange",
    type: ExchangeType.Fanout,
    durable: true,
    cancellationToken: cancellationToken);

            await _channel.QueueDeclareAsync(
                queue: "email-orders",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);

            await _channel.QueueBindAsync(
                queue: "email-orders",
                exchange: "order-created-exchange",
                routingKey: "",
                cancellationToken: cancellationToken);

            await base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_channel == null)
                throw new InvalidOperationException("RabbitMQ channel is not initialized");

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (_, e) =>
            {
                using var scope = _scopeFactory.CreateScope();

                try
                {
                    var json = Encoding.UTF8.GetString(e.Body.ToArray());
                    var order = JsonSerializer.Deserialize<OrderCreatedEvent>(json);

                    if (order is null)
                    {
                        await _channel.BasicAckAsync(e.DeliveryTag, false);
                        return;
                    }

                    var userClient =
                        scope.ServiceProvider.GetRequiredService<UserService.UserServiceClient>();

                    var emailService =
                        scope.ServiceProvider.GetRequiredService<IEmailService>();

                    var user = await userClient.GetUserAsync(new UserRequest
                    {
                        UserId = order.UserId
                    });

                    await emailService.SendEmailAsync(
    user.Email,
    "Замовлення успішно створено",
    $@"
    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
        
        <div style='background:#f97316; padding:20px; text-align:center;'>
            <h1 style='color:white; margin:0;'>
                Auto Service
            </h1>
        </div>

        <div style='padding:30px; background:#f9fafb;'>

            <h2 style='color:#111827;'>
                Вітаємо, {user.Username}!
            </h2>

            <p style='font-size:16px; color:#374151;'>
                Ваше замовлення успішно створено.
            </p>

            <div style='background:white;
                        border:1px solid #e5e7eb;
                        border-radius:10px;
                        padding:20px;
                        margin:20px 0;'>

                <p><strong>Номер замовлення:</strong> #{order.OrderId}</p>
                <p><strong>Дата:</strong> {order.OrderDate:dd.MM.yyyy}</p>
                <p><strong>Час:</strong> {order.OrderDate:HH:mm}</p>

            </div>

            <p style='color:#6b7280;'>
                Дякуємо за вибір нашого сервісу.
            </p>

        </div>

        <div style='background:#111827;
                    color:white;
                    padding:15px;
                    text-align:center;'>

            © 2026 Auto Service

        </div>

    </div>"
);

                    await _channel.BasicAckAsync(e.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Notification ERROR] {ex.Message}");

                    await _channel.BasicNackAsync(
                        e.DeliveryTag,
                        multiple: false,
                        requeue: true);
                }
            };

            _channel.BasicConsumeAsync(
    queue: "email-orders",
    autoAck: false,
    consumer: consumer
);

            return Task.CompletedTask;
        }

        public override void Dispose() { _channel?.Dispose(); _connection?.Dispose(); base.Dispose(); }
    }
}