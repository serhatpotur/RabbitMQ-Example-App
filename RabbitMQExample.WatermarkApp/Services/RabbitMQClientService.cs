using RabbitMQ.Client;

namespace RabbitMQExample.WatermarkApp.Services
{
    public class RabbitMQClientService : IDisposable
    {
        public RabbitMQClientService(ConnectionFactory connectionFactory, ILogger<RabbitMQClientService> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }
        private readonly ConnectionFactory _connectionFactory;
        private readonly ILogger<RabbitMQClientService> _logger;

        private IConnection _connection;
        private IModel _channel;
        public static string ExchangeName = "direct-exchange-image";
        public static string RoutingWatermark = "watermark-route-image";
        public static string QueueName = "queue-watermark-image";

        public IModel Connect()
        {
            _connectionFactory.DispatchConsumersAsync = true; 
            _connection = _connectionFactory.CreateConnection();
            if (_channel is { IsOpen: true }) // if(_channel.isOpen==true) 
                return _channel;

            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(exchange: ExchangeName, type: ExchangeType.Direct, durable: true, autoDelete: false);

            _channel.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

            _channel.QueueBind(queue: QueueName, exchange: ExchangeName, routingKey: RoutingWatermark, arguments: null);

            _logger.LogInformation("RabbitMQ ile bağlantı kuruldu");

            return _channel;


        }

        public void Dispose()
        {
            _channel?.Close(); // channel var ise kapat
            _channel?.Dispose(); //channel var ise dispose olsun
            _connection?.Close();
            _connection?.Dispose();

            _logger.LogInformation("RabbitMQ ile bağlatı kapatıldı");
        }
    }
}
