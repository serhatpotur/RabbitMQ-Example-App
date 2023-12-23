﻿using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace RabbitMQExample.WatermarkApp.Services
{
    public class RabbitMQPublisher
    {
        private readonly RabbitMQClientService _rabbitmqClientService;

        public RabbitMQPublisher(RabbitMQClientService rabbitmqClientService)
        {
            _rabbitmqClientService = rabbitmqClientService;
        }
        public void Publish(ProductImageCreatedEvent createdEvent)
        {
            var channel = _rabbitmqClientService.Connect();

            var bodyString = JsonSerializer.Serialize(createdEvent);
            var bodyByte = Encoding.UTF8.GetBytes(bodyString);

            var property = channel.CreateBasicProperties();
            property.Persistent = true;

            channel.BasicPublish(
                exchange: RabbitMQClientService.ExchangeName,
                routingKey: RabbitMQClientService.RoutingWatermark,
                basicProperties: property,
                body: bodyByte
                );
        }
    }
}
