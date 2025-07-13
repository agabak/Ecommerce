using Blazored.LocalStorage;
using Confluent.Kafka;
using Ecom_Client.Components;
using Ecom_Client.Services;
using Ecom_Client.Services.https;
using Ecommerce.Common.Services.Kafka;
using Ecommerce.Common.Settings.Kafka;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
       .AddInteractiveServerComponents();

builder.Services.AddHttpClient<IProductService, ProductService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7063/");
});

builder.Services.AddHttpClient<IAuthService, AuthService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7254/");
});

 builder.Services.AddBlazoredLocalStorage();

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


builder.Services.AddSingleton<ICartService, CartService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
