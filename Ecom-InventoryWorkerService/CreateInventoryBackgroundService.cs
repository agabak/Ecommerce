using Ecom_InventoryWorkerService.Services;
using Ecommerce.Common.Models;
using Ecommerce.Common.Services.Kafka;
using System.Text.Json;

namespace Ecom_InventoryWorkerService;

public class CreateInventoryBackgroundService : BackgroundService
{
    private readonly ILogger<CreateInventoryBackgroundService> _logger;
    private readonly IConsumerService _consumerService;
    private readonly IServiceProvider _serviceProvider;
    private const string Topic_Create_Inventory = "Create-Inventory";
   
    public CreateInventoryBackgroundService(
        ILogger<CreateInventoryBackgroundService> logger,
        IConsumerService consumerService,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _consumerService = consumerService;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("InventoryConsumerService started. Subscribing to topics...");
        await ProcessCreateInventory(stoppingToken);
    }

    private async Task ProcessCreateInventory(CancellationToken cancellationToken)
    {
        await _consumerService.ProcessAsync(
           topic: Topic_Create_Inventory,
           messageHandler: async (message) =>
           {
               await HandleCreateInventoryMessageAsync(message, cancellationToken);
           },
           cancellationToken: cancellationToken
       );
    }

    private async Task HandleCreateInventoryMessageAsync(string message, CancellationToken token)
    {
        using var scope = _serviceProvider.CreateScope();
        var inventoryService = scope.ServiceProvider.GetRequiredService<IInventoryService>();

        // Trim whitespace and try to parse productId
        if (!Guid.TryParse(message?.Trim(), out var productId))
        {
            _logger.LogWarning("Received invalid productId in message: {Message}", message);
            return;
        }

        try
        {
            await inventoryService.UpsertInventoryAsync(productId, token);
            _logger.LogInformation("Successfully processed inventory message for productId: {ProductId}", productId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upsert inventory for productId: {ProductId}, message: {Message}", productId, message);
        }
    }
}
