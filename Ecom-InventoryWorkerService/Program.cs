using Ecom_InventoryWorkerService;
using Ecommerce.Common.DataAccess;
using Ecommerce.Common.DataAccess.Inventory.Repositories;
using Ecommerce.Common.DataAccess.Inventory.Services;
using Ecommerce.Common.Settings.Extension;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IDataAccessProvider>
    (_ => new InventoryDataAccessProvider(builder.Configuration.GetConnectionString("DefaultConnection")!)); // Updated to use Microsoft.Data.SqlClient.SqlConnection

builder.Services.AddKafkaConsumerProducer(builder.Configuration);

builder.Services.AddHostedService<CreateInventoryBackgroundService>();

builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IInventoryService, InventoryService>();


var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
