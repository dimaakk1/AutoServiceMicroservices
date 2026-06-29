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

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException(
                    "RabbitMQ connection string not found.");
            }

            var factory = new ConnectionFactory
            {
                Uri = new Uri(connectionString)
            };

            await using var connection =
                await factory.CreateConnectionAsync();

            await using var channel =
                await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue: "order-created",
                durable: true,
                exclusive: false,
                autoDelete: false);

            var body = Encoding.UTF8.GetBytes(
                JsonSerializer.Serialize(order));

            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: "order-created",
                body: body);
        }
    }
}
