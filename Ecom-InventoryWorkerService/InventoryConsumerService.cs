using Ecom_InventoryWorkerService.Services;
using Ecommerce.Common.Services.Kafka;

namespace Ecom_InventoryWorkerService;

public class InventoryConsumerService : BackgroundService
{
    private readonly ILogger<InventoryConsumerService> _logger;
    private readonly IConsumerService _consumerService;
    private readonly IServiceProvider _serviceProvider;
    private const string Topic = "Create-Inventory";

    public InventoryConsumerService(
        ILogger<InventoryConsumerService> logger,
        IConsumerService consumerService,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _consumerService = consumerService;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("InventoryConsumerService started. Subscribing to topic: {Topic}", Topic);

        // Pass message handler delegate to your reusable consumer service
        await _consumerService.ProcessAsync(
            topic: Topic,
            messageHandler: async (message) =>
            {
                await HandleMessageAsync(message, stoppingToken);
            },
            cancellationToken: stoppingToken
        );
    }

    private async Task HandleMessageAsync(string message, CancellationToken token)
    {
        using var scope = _serviceProvider.CreateScope();   
        var inventoryService = scope.ServiceProvider.GetRequiredService<IInventoryService>();

        // Parse productId from the message (assume it's a GUID string)
        if (!Guid.TryParse(message, out var productId))
        {
            _logger.LogWarning("Received invalid productId in message: {Message}", message);
            return;
        }

        await inventoryService.UpsertInventoryAsync(productId, token);

        _logger.LogInformation("Processed inventory message for productId: {ProductId}", productId);
    }
}
