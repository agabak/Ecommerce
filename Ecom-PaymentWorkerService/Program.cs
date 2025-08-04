using ECom.Infrastructure.DataAccess;
using ECom.Infrastructure.DataAccess.Order;
using ECom.Infrastructure.DataAccess.Order.Repositories;
using ECom.Infrastructure.DataAccess.Order.Services;
using Ecom_PaymentWorkerService;
using Ecommerce.Common.Settings.Extension;

var builder = WebApplication.CreateBuilder(args);

// IDbConnection 
builder.Services.AddScoped<IOrderRepository>(sp => {
    return new OrderRepository(builder.Configuration.GetConnectionString("DefaultConnection")!);
});

builder.Services.AddScoped<IPaymentRepository>(sp => {
    return new PaymentRepository(builder.Configuration.GetConnectionString("DefaultConnection")!);
});

builder.Services.AddKafkaConsumerProducer(builder.Configuration);

// Add other services as needed
builder.Services.AddHostedService<PaymentBackgroundWorkerService>();
builder.Services.AddScoped<IOrderService, OrderService>();
//builder.Services.AddScoped<IOrderRepository, OrderRepository>();
//builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
