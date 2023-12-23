using ClosedXML.Excel;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQExample.FileCreateWorkerService.Models;
using RabbitMQExample.FileCreateWorkerService.Services;
using RabbitMQExample.Shared;
using System.Data;
using System.Text;
using System.Text.Json;

namespace RabbitMQExample.FileCreateWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly RabbitMQClientService _rabbitMQClientService;
        private readonly IServiceProvider _serviceProvider;
        private IModel _channel;

        public Worker(ILogger<Worker> logger, RabbitMQClientService rabbitMQClientService, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _rabbitMQClientService = rabbitMQClientService;
            _serviceProvider = serviceProvider;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {

            _channel = _rabbitMQClientService.Connect();
            _channel.BasicQos(0, 1, false);

            return base.StartAsync(cancellationToken);
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {

            var consumer = new AsyncEventingBasicConsumer(_channel);

            _channel.BasicConsume(queue: RabbitMQClientService.QueueName, autoAck: false, consumer: consumer);

            consumer.Received += Consumer_Received;

            return Task.CompletedTask;
        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
        {
            await Task.Delay(5000);
            try
            {

                var bodyAsString = Encoding.UTF8.GetString(@event.Body.ToArray());

                var createExcelMessage = JsonSerializer.Deserialize<CreateExcelMessage>(bodyAsString);


                using var ms = new MemoryStream();
                var wb = new XLWorkbook();
                var ds = new DataSet();
                ds.Tables.Add(GetTable("Test"));
                wb.Worksheets.Add(ds);
                wb.SaveAs(ms);

                MultipartFormDataContent dataContent = new();
                // file : IFormFile parametresinin adý olmalý
                dataContent.Add(content: new ByteArrayContent(ms.ToArray()), name: "file", fileName: Guid.NewGuid().ToString() + ".xlsx");

                var baseUrl = "https://localhost:7195/api/files";
                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.PostAsync($"{baseUrl}?fileId={createExcelMessage.FileId}", dataContent);
                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation($"File (Id : {createExcelMessage.FileId}) was created by successful");
                        _channel.BasicAck(@event.DeliveryTag, false);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message.ToString());

            }
           


        }

        private DataTable GetTable(string tableName)
        {
            List<RabbitMqexcelTestTable> result;
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<RabbitMqCreateExcelDbContext>();
                result = context.RabbitMqexcelTestTables.ToList();
            }

            DataTable dataTable = new DataTable
            {
                TableName = tableName,
            };
            dataTable.Columns.Add("Id", typeof(int));
            dataTable.Columns.Add("Name", typeof(string));
            dataTable.Columns.Add("Description", typeof(string));

            result.ForEach(x =>
            {
                dataTable.Rows.Add(x.Id, x.Name, x.Description);
            });
            return dataTable;
        }
    }
}