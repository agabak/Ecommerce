using Ecom_PaymentWorkerService;
using Ecom_PaymentWorkerService.Repositories;
using Ecom_PaymentWorkerService.Services;
using Ecommerce.Common.Settings.Extension;
using Microsoft.Data.SqlClient;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

// IDbConnection 
builder.Services.AddScoped<IDbConnection>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    return new SqlConnection(connectionString);
});

builder.Services.AddKafkaConsumerProducer(builder.Configuration);

// Add other services as needed
builder.Services.AddHostedService<PaymentBackgroundWorkerService>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
