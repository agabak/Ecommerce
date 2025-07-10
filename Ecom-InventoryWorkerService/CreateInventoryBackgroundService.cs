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
    private readonly IKafkaTopicCheckerService _kafkaTopicCheckerService;
    private const string Topic_Create_Inventory = "Create-Inventory";
   
    public CreateInventoryBackgroundService(
        ILogger<CreateInventoryBackgroundService> logger,
        IConsumerService consumerService,
        IServiceProvider serviceProvider,
        IKafkaTopicCheckerService kafkaTopicCheckerService)
    {
        _logger = logger;
        _consumerService = consumerService;
        _serviceProvider = serviceProvider;
        _kafkaTopicCheckerService=kafkaTopicCheckerService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Check if the topic exists before starting the consumer
       if(!_kafkaTopicCheckerService.TopicExists(Topic_Create_Inventory))
        {
            _logger.LogError("Kafka topic '{Topic}' does not exist. Aborting consumer start.", Topic_Create_Inventory);
            return;
        }

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
