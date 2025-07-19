using Azure.Storage.Blobs;
using ECom.Infrastructure.DataAccess;
using ECom.Infrastructure.DataAccess.Product;
using ECom.Infrastructure.DataAccess.Product.Repositories;
using ECom.Infrastructure.DataAccess.Product.Services;
using Ecommerce.Common.Middleware;
using Ecommerce.Common.Services.Files;
using Ecommerce.Common.Settings.Extension;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddScoped<IDataAccessProvider>
    (_ => new ProductDataAccessProvider(builder.Configuration.GetConnectionString("DefaultConnection")!)); // Updated to use Microsoft.Data.SqlClient.SqlConnection


builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var connectionString = config["AzureBlobStorage:ConnectionString"];
    var containerName = config["AzureBlobStorage:ContainerName"];
    return new BlobContainerClient(connectionString, containerName);
});

builder.Services.AddSingleton<IBlobService, BlobService>();

builder.Services.AddKafkaConsumerProducer(builder.Configuration);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionHandler>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
