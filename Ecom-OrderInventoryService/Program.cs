using ECom.Infrastructure.DataAccess;
using ECom.Infrastructure.DataAccess.Inventory;
using ECom.Infrastructure.DataAccess.Inventory.Repositories;
using ECom.Infrastructure.DataAccess.Inventory.Services;
using Ecom_OrderInventoryService;
using Ecommerce.Common.Settings.Extension;

var builder = WebApplication.CreateBuilder(args);

// IDbConnection setup
builder.Services.AddScoped<IDataAccessProvider>
    (_ => new InventoryDataAccessProvider(builder.Configuration.GetConnectionString("DefaultConnection")!)); 

builder.Services.AddKafkaConsumerProducer(builder.Configuration);

builder.Services.AddHostedService<ProcessInventoryOrderBackgroundService>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IInventoryService, InventoryService>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
