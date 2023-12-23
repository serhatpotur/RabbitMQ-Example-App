// Publisher

using RabbitMQ.Client;
using RabbitMQExample.Shared;
using System.Text;
using System.Text.Json;

var factory = new ConnectionFactory();
factory.Uri = new Uri("amqps://xdeobiyn:crbb-OcTyRzAijqE0osMdy6TMmZAc00w@sparrow.rmq.cloudamqp.com/xdeobiyn");

using var connection = factory.CreateConnection();

var channel = connection.CreateModel();
channel.ExchangeDeclare("header-exchange", durable: true, type: ExchangeType.Headers);
Dictionary<string, object> headers = new Dictionary<string, object>();

headers.Add("format", "pdf");
headers.Add("shape2", "a4");

var properties = channel.CreateBasicProperties();
properties.Headers = headers;
properties.Persistent = true;
//durable : queue kayıt edilsin mi
//exclusşve : bu kuruğa sadece bu kanal üzerinden bağlan
// son subs düşerse kuyruk silinsin mi
//channel.QueueDeclare(queue: "hello-queue", durable: true, exclusive: false, autoDelete: false);

var product = new Product { Id = 1, Name = "Kalem", Price = 100, Stock = 10 };
var productJsonString = JsonSerializer.Serialize(product);

channel.BasicPublish("header-exchange", string.Empty, properties, Encoding.UTF8.GetBytes(productJsonString));

Console.WriteLine("mesaj gönderilmiştir");


Console.ReadLine();