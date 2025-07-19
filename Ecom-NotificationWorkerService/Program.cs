using ECom.Infrastructure.DataAccess;
using ECom.Infrastructure.DataAccess.Order;
using ECom.Infrastructure.DataAccess.Order.Repositories;
using ECom.Infrastructure.DataAccess.Order.Services;
using Ecom_NotificationWorkerService;
using Ecommerce.Common.Settings.Extension;

var builder = WebApplication.CreateBuilder(args);

// IDbConnection
builder.Services.AddScoped<IDataAccessProvider>(sp =>
{
    return new OrderDataAccessProvider(builder.Configuration.GetConnectionString("DefaultConnection")!);
});

builder.Services.AddKafkaConsumerProducer(builder.Configuration);

builder.Services.AddHostedService<ConfirmationBackgroundWorkerService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>(); 
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
