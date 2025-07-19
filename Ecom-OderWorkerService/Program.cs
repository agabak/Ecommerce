using ECom.Infrastructure.DataAccess;
using ECom.Infrastructure.DataAccess.Order;
using ECom.Infrastructure.DataAccess.Order.Repositories;
using ECom.Infrastructure.DataAccess.Order.Services;
using Ecom_OderWorkerService;
using Ecommerce.Common.Settings.Extension;

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
