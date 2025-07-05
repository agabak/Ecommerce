using Confluent.Kafka;
using Ecom_InventoryWorkerService;
using Ecom_InventoryWorkerService.Repositories;
using Ecom_InventoryWorkerService.Services;
using Microsoft.Data.SqlClient;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IDbConnection>
    (_ => new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"))); // Updated to use Microsoft.Data.SqlClient.SqlConnection

var kafkaConsumerSettings = builder.Configuration
    .GetSection("KafkaConsumer")
    .Get<KafkaConsumerSettings>();

builder.Services.AddSingleton<ConsumerConfig>(kafkaConsumerSettings);
builder.Services.AddHostedService<InventoryConsumerService>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IInventoryService, InventoryService>();


var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
