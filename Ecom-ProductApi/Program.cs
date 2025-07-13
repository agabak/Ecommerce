using Confluent.Kafka;
using Ecom_ProductApi.Repositories;
using Ecom_ProductApi.Services;
using Ecommerce.Common.Services.Files;
using Ecommerce.Common.Services.Kafka;
using Ecommerce.Common.Settings.Kafka;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddScoped<IDbConnection>
    (_ => new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"))); // Updated to use Microsoft.Data.SqlClient.SqlConnection

builder.Services.AddSingleton<IBlobService>
                (new BlobService(builder.Configuration.GetConnectionString("blobConnectionString"), "files"));


// Bind and register ConsumerSettings
builder.Services.Configure<ConsumerSettings>(
    builder.Configuration.GetSection("ConsumerSettings"));

// Bind and register ProducerSettings
builder.Services.Configure<ProducerSettings>(
    builder.Configuration.GetSection("ProducerSettings"));

// (Optional) Register strongly-typed settings for direct injection
builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IOptions<ConsumerSettings>>().Value);
builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IOptions<ProducerSettings>>().Value);

// Register Kafka consumer using DI-bound settings
builder.Services.AddSingleton<IConsumer<string, string>>(sp =>
{
    var settings = sp.GetRequiredService<ConsumerSettings>();
    return new ConsumerBuilder<string, string>(settings).Build();
});

// Register Kafka producer using DI-bound settings
builder.Services.AddSingleton<IProducer<string, string>>(sp =>
{
    var settings = sp.GetRequiredService<ProducerSettings>();
    return new ProducerBuilder<string, string>(settings).Build();
});

builder.Services.AddSingleton<IProducerService, ProducerService>();
builder.Services.AddSingleton<IConsumerService, ConsumerService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IKafkaProducerService, KafkaProducerService>();
builder.Services.AddHostedService<DailyInventoryJobService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
