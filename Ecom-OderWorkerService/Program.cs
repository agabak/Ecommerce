using Confluent.Kafka;
using Ecom_OderWorkerService;
using Ecom_OderWorkerService.Repositories;
using Ecom_OderWorkerService.Services;
using Ecommerce.Common.DataAccess;
using Ecommerce.Common.DataAccess.Order;
using Ecommerce.Common.Services.Kafka;
using Ecommerce.Common.Settings.Extension;
using Ecommerce.Common.Settings.Kafka;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

// inject IDbConnection for SQL Server
builder.Services.AddScoped<IDataAccessProvider>
    (_ => new OrderDataAccessProvider(builder.Configuration.GetConnectionString("DefaultConnection")!)); // Updated to use Microsoft.Data.SqlClient.SqlConnection

builder.Services.AddKafkaConsumerProducer(builder.Configuration);

// Add other services as needed
builder.Services.AddHostedService<CreateOrderBackgroundService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();

var app = builder.Build();
app.MapGet("/", () => "Hello World!");

app.Run();
