//Subsriber

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQExample.Shared;
using System.Text;
using System.Text.Json;

var factory = new ConnectionFactory();
factory.Uri = new Uri("amqps://xdeobiyn:crbb-OcTyRzAijqE0osMdy6TMmZAc00w@sparrow.rmq.cloudamqp.com/xdeobiyn");

using var connection = factory.CreateConnection();

var channel = connection.CreateModel();
channel.ExchangeDeclare("header-exchange", durable: true, type: ExchangeType.Headers);

channel.BasicQos(0, 1, false);
var consumer = new EventingBasicConsumer(channel);

var queueName = channel.QueueDeclare().QueueName;
Dictionary<string, object> headers = new Dictionary<string, object>();

headers.Add("format", "pdf");
headers.Add("shape", "a4");
headers.Add("x-match", "any");
channel.QueueBind(queueName, "header-exchange", String.Empty, headers);

// autoAck :true mesaj doğruda yanlışda işlense kuyruktan siler, false ise mesaj doğru işlenirse ben sana söyliyeceğim o zaman sil
channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
Console.WriteLine("Loglar dinleniyor...");

consumer.Received += (object sender, BasicDeliverEventArgs e) =>
{
    var message = Encoding.UTF8.GetString(e.Body.ToArray());

    Product product = JsonSerializer.Deserialize<Product>(message);

    Thread.Sleep(1500);
    Console.WriteLine($"Gelen Mesaj: {product.Id}-{product.Name}-{product.Price}-{product.Stock}");


    //DeliveryTag : Hangi mesajı sileceğimizi söyleriz
    //multiple : true işlenmiş ama rabiite gitmeyen mesajları haberdar eder.false sadece ilgili mesajı kotrol eder
    channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);
};


Console.ReadLine();