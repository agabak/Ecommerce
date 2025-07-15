using Ecom_OrderInventoryService;
using Ecom_OrderInventoryService.Repositories;
using Ecom_OrderInventoryService.Services;
using Ecommerce.Common.Settings.Extension;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

// IDbConnection setup
builder.Services.AddScoped<IDbConnection>
    (_ => new Microsoft.Data.SqlClient.SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"))); 

builder.Services.AddKafkaConsumerProducer(builder.Configuration);

builder.Services.AddHostedService<ProcessInventoryOrderBackgroundService>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IInventoryService, InventoryService>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
