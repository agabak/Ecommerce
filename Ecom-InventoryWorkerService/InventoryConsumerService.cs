using Confluent.Kafka;
using Ecom_InventoryWorkerService.Services;

namespace Ecom_InventoryWorkerService;

public class InventoryConsumerService : BackgroundService
{
    private readonly ILogger<InventoryConsumerService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ConsumerConfig _config;
    private const string Topic = "Create-Inventory";

    public InventoryConsumerService(
        ILogger<InventoryConsumerService> logger,
        ConsumerConfig config,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _config = config;
        _serviceProvider=serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var consumer = new ConsumerBuilder<Ignore, string>(_config).Build();
        consumer.Subscribe(Topic);

        _logger.LogInformation("Started Kafka consumer for topic: {Topic}", Topic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = consumer.Consume(stoppingToken);

                    _logger.LogInformation("Received message: {Message}", result.Message.Value);

                    // TODO: Call your inventory creation logic here
                    await HandleMessageAsync(result.Message.Value, stoppingToken);
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Kafka consume error: {Reason}", ex.Error.Reason);
                }
            }
        }
        finally
        {
            consumer.Close();
        }
    }

    private async Task HandleMessageAsync(string message, CancellationToken token)
    {
        // Example: parse and log (or call inventory service)
        using var scope = _serviceProvider.CreateScope();
        var inventoryService = scope.ServiceProvider.GetRequiredService<IInventoryService>();
       
        Guid.TryParse(message, out var productId);

        await inventoryService.UpsertInventoryAsync(productId, token);

        _logger.LogInformation("Processing inventory message: {Message}", message);
    }
}



