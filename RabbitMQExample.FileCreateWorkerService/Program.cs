using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQExample.FileCreateWorkerService;
using RabbitMQExample.FileCreateWorkerService.Models;
using RabbitMQExample.FileCreateWorkerService.Services;

//IHost host = Host.CreateDefaultBuilder(args)
//    .ConfigureServices(services =>
//    {
//        services.AddHostedService<Worker>();
//    })
//    .Build();

//await host.RunAsync();

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        IConfiguration Configuration = context.Configuration;
        services.AddDbContext<RabbitMqCreateExcelDbContext>(options =>
        {
            options.UseSqlServer(Configuration.GetConnectionString("SqlServer"));
        });

        services.AddSingleton(sp => new ConnectionFactory() { Uri = new Uri(Configuration.GetConnectionString("RabbitMQ")), DispatchConsumersAsync = true });
        services.AddSingleton<RabbitMQClientService>();

        services.AddHostedService<Worker>();
    })
    .Build();


await host.RunAsync();