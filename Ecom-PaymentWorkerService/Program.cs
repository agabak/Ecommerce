using Ecom_PaymentWorkerService;
using Ecom_PaymentWorkerService.Repositories;
using Ecom_PaymentWorkerService.Services;
using Ecommerce.Common.DataAccess;
using Ecommerce.Common.DataAccess.Order;
using Ecommerce.Common.Settings.Extension;

var builder = WebApplication.CreateBuilder(args);

// IDbConnection 
builder.Services.AddScoped<IDataAccessProvider>(sp => {
    return new OrderDataAccessProvider(builder.Configuration.GetConnectionString("DefaultConnection")!);
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
