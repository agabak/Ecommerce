
using Ecom_InventoryWorkerService.Services;
using Ecommerce.Common.Models;
using Ecommerce.Common.Services.Kafka;
using System.Text.Json;

namespace Ecom_InventoryWorkerService;

public class ProcessInventoryOrderBackgroundService : BackgroundService
{
    private readonly ILogger<ProcessInventoryOrderBackgroundService> _logger;
    private readonly IConsumerService _consumerService;
    private readonly IServiceProvider _serviceProvider;
    private const string Topic_Order_Created = "Order-Created";
    private const string Topic_Order_Inventory_Created = "Order-Inventory-Created";

    public ProcessInventoryOrderBackgroundService(
        ILogger<ProcessInventoryOrderBackgroundService> logger,
        IConsumerService consumerService,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _consumerService = consumerService;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      
         _logger.LogInformation("ProcessInventoryOrderBackgroundService started. Subscribing to topics...");
        await ProcessOrderCreate(stoppingToken);
    }

    private async Task ProcessOrderCreate(CancellationToken cancellationToken)
    {
        await _consumerService.ProcessAsync(
           topic: Topic_Order_Created,
           messageHandler: async (message) =>
           {
               await HandleOrderCreatedMessageAsync(message, cancellationToken);
           },
           cancellationToken: cancellationToken
       );
    }

    private async Task HandleOrderCreatedMessageAsync(string message, CancellationToken token)
    {
        using var scope = _serviceProvider.CreateScope();
        var inventoryService = scope.ServiceProvider.GetRequiredService<IInventoryService>();
        var productService = scope.ServiceProvider.GetRequiredService<IProducerService>();

        List<Item>? items = null;

        try
        {
            items = JsonSerializer.Deserialize<List<Item>>(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize OrderCreated message: {Message}", message);
            return;
        }

        if (items == null || !items.Any())
        {
            _logger.LogWarning("Received empty or invalid cart items in message: {Message}", message);
            return;
        }

        try
        {
            var result = await inventoryService.UpdateInventoryAfterOrderAsync(items, token);

            foreach (var item in items)
            {
                result.TryGetValue(item.Product.ProductId, out var warehouseId);

                var order = new Order
                {
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = item.TotalPrice,
                    OrderItems = new List<OrderItem>
                    {
                        new OrderItem
                        {
                            ProductId = item.Product.ProductId,
                            Quantity = item.Quantity,
                            UnitPrice = item.Product.Price,
                            TotalPrice = item.TotalPrice,
                            WarehouseId = warehouseId
                        }
                    }
                };

                var messageContent = JsonSerializer.Serialize(order);

                // Send the order to the Order-Inventory-Created topic
                await productService.ProduceAsync(Topic_Order_Inventory_Created, messageContent);

                _logger.LogInformation("Order for ProductId: {ProductId} sent to WarehouseId: {WarehouseId}",
                    item.Product.ProductId, warehouseId);
            }

            _logger.LogInformation("Processed order created message with {Count} items", items.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating inventory for order items: {Message}", message);
        }
    }
}
