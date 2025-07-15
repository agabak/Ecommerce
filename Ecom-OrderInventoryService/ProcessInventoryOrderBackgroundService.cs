
using Ecom_OrderInventoryService.Services;
using Ecommerce.Common.Models;
using Ecommerce.Common.Services.Kafka;
using System.Text.Json;

namespace Ecom_OrderInventoryService;

public class ProcessInventoryOrderBackgroundService(
    ILogger<ProcessInventoryOrderBackgroundService> _logger,
    IConsumerService _consumerService,
    IServiceProvider _serviceProvider
    ) : BackgroundService
{

    private const string Topic_Order_Created = "Order.Created";
    private const string Topic_Inventory_Reserved = "Inventory.Reserved";
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
                try
                {
                    await HandleOrderCreatedMessageAsync(message, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error handling OrderCreated message: {Message}", message);
                }
            },
            cancellationToken: cancellationToken
        );
    }

    private async Task HandleOrderCreatedMessageAsync(string message, CancellationToken token)
    {
        using var scope = _serviceProvider.CreateScope();
        var inventoryService = scope.ServiceProvider.GetRequiredService<IInventoryService>();
        var productService = scope.ServiceProvider.GetRequiredService<IProducerService>();

        Cart? cart = null;

        try
        {
            cart = JsonSerializer.Deserialize<Cart>(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize OrderCreated message: {Message}", message);
            throw;
        }

        if (cart == null || !cart.Items.Any())
        {
            _logger.LogWarning("Received empty or invalid cart items in message: {Message}", message);
            return;
        }

        try
        {
            var result = await inventoryService.UpdateInventoryAfterOrderAsync(cart.Items, token);

            foreach (var item in cart.Items)
            {
                result.TryGetValue(item.Product.ProductId, out var warehouseId);

                var order = new Order
                {
                    User = cart.User,
                    ShippingAddress = cart.User.Address,
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = item.TotalPrice,
                    PaymentType = cart.PaymentType,
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
                await productService.ProduceAsync(Topic_Inventory_Reserved, messageContent);

                _logger.LogInformation("Order for ProductId: {ProductId} sent to WarehouseId: {WarehouseId}",
                    item.Product.ProductId, warehouseId);
            }

            _logger.LogInformation("Processed order created message with {Count} items", cart.Items.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating inventory for order items: {Message}", message);
        }
    }
}
