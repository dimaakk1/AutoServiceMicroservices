using AutoserviceOrders.BLL.DTO;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AutoserviceOrders.BLL.Services
{
    public class RabbitMqPublisher
    {
        private readonly IConfiguration _configuration;

        public RabbitMqPublisher(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task PublishOrderCreatedAsync(OrderCreatedEvent order)
        {
            var connectionString =
                _configuration.GetConnectionString("rabbitmq");

            var factory = new ConnectionFactory
            {
                Uri = new Uri(connectionString)
            };

            await using var connection =
                await factory.CreateConnectionAsync();

            await using var channel =
                await connection.CreateChannelAsync();

            await channel.ExchangeDeclareAsync(
                exchange: "order-created-exchange",
                type: ExchangeType.Fanout,
                durable: true);

            var body = Encoding.UTF8.GetBytes(
                JsonSerializer.Serialize(order));

            await channel.BasicPublishAsync(
                exchange: "order-created-exchange",
                routingKey: "",
                body: body);

            Console.WriteLine(
                $"Order {order.OrderId} published");
        }
    }
}
