using Confluent.Kafka;
using Ecom_ProductApi;
using Ecom_ProductApi.Repositories;
using Ecom_ProductApi.Services;
using Ecommerce.Common.Services.Files;
using System.Data;
using System.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddScoped<IDbConnection>
    (_ => new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"))); // Updated to use Microsoft.Data.SqlClient.SqlConnection

builder.Services.AddSingleton<IBlobService>
                (new BlobService(builder.Configuration.GetConnectionString("blobConnectionString"), "files"));


// Bind Kafka settings from configuration
var kafkaSettings = builder.Configuration.GetSection("Kafka").Get<KafkaSettings>();
builder.Services.AddSingleton<ProducerConfig>(kafkaSettings);

// Register the producer (Null key, string value)
builder.Services.AddSingleton<IProducer<Null, string>>(provider =>
{
    var config = provider.GetRequiredService<ProducerConfig>();
    return new ProducerBuilder<Null, string>(config)
        .SetValueSerializer(Serializers.Utf8) // optional, can swap out for JSON/Avro
        .Build();
});

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
