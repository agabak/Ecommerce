using Confluent.Kafka;
using Ecom_InventoryWorkerService;
using Ecom_InventoryWorkerService.Repositories;
using Ecom_InventoryWorkerService.Services;
using Ecommerce.Common.Services.Kafka;
using Ecommerce.Common.Settings.Kafka;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IDbConnection>
    (_ => new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"))); // Updated to use Microsoft.Data.SqlClient.SqlConnection

// Bind and register ConsumerSettings
builder.Services.Configure<ConsumerSettings>(
    builder.Configuration.GetSection("ConsumerSettings"));

// Bind and register ProducerSettings
builder.Services.Configure<ProducerSettings>(
    builder.Configuration.GetSection("ProducerSettings"));

// (Optional) Register strongly-typed settings for direct injection
builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IOptions<ConsumerSettings>>().Value);
builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IOptions<ProducerSettings>>().Value);

// Register Kafka consumer using DI-bound settings
builder.Services.AddSingleton<IConsumer<Null, string>>(sp =>
{
    var settings = sp.GetRequiredService<ConsumerSettings>();
    return new ConsumerBuilder<Null, string>(settings).Build();
});

// Register Kafka producer using DI-bound settings
builder.Services.AddSingleton<IProducer<Null, string>>(sp =>
{
    var settings = sp.GetRequiredService<ProducerSettings>();
    return new ProducerBuilder<Null, string>(settings).Build();
});

builder.Services.AddSingleton<IProducerService, ProducerService>();
builder.Services.AddSingleton<IConsumerService, ConsumerService>();


builder.Services.AddHostedService<InventoryConsumerService>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IInventoryService, InventoryService>();


var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
