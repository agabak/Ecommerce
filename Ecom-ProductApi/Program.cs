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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
