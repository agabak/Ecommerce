using ECom.Infrastructure.DataAccess.Order.Repositories;
using ECom.Infrastructure.DataAccess.Order.Services;
using Ecom_NotificationWorkerService;
using Ecommerce.Common.Settings.Extension;

var builder = WebApplication.CreateBuilder(args);

// IDbConnection
builder.Services.AddScoped<INotificationRepository>(sp =>
{
    return new NotificationRepository(builder.Configuration.GetConnectionString("DefaultConnection")!);
});

builder.Services.AddScoped<IOrderRepository>(sp =>
{
    return new OrderRepository(builder.Configuration.GetConnectionString("DefaultConnection")!);
});

builder.Services.AddKafkaConsumerProducer(builder.Configuration);

builder.Services.AddHostedService<ConfirmationBackgroundWorkerService>();
//builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>(); 
//builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
