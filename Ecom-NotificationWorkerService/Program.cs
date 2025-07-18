using Ecom_NotificationWorkerService;
using Ecom_NotificationWorkerService.DataAccess;
using Ecom_NotificationWorkerService.Repositories;
using Ecom_NotificationWorkerService.Services;
using Ecommerce.Common.Settings.Extension;

var builder = WebApplication.CreateBuilder(args);

// IDbConnection
builder.Services.AddScoped<IOrderDataAccessProvider>(sp =>
{
    return new OrderDataAccessProvider(builder.Configuration.GetConnectionString("DefaultConnection")!);
});

builder.Services.AddKafkaConsumerProducer(builder.Configuration);

builder.Services.AddHostedService<ConfirmationBackgroundWorkerService>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();    

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
