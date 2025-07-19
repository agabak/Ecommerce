using Confluent.Kafka;
using ECom.Infrastructure.DataAccess.Inventory.Services;
using Ecommerce.Common.Models;
using Ecommerce.Common.Services.Kafka;
using System.Text.Json;

namespace Ecom_OrderInventoryService;

public class ProcessInventoryOrderBackgroundService(
    ILogger<ProcessInventoryOrderBackgroundService> _logger,
    IConsumerService _consumerService,
    IProducerService _producerService,
    IServiceProvider _serviceProvider
    ) : BackgroundService
{

    private const string Topic_Order_Created = "Order.Created";
    private const string Topic_Inventory_Reserved = "Inventory.Reserved";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ProcessInventoryOrderBackgroundService started. Subscribing to topics...");

        await _consumerService.ProcessAsync(
            topic: Topic_Order_Created,
            messageHandler: async (message) =>
            {
                try
                {
                    await HandleOrderCreatedMessageAsync(message, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error handling OrderCreated message: {Message}", message);
                }
            },
            cancellationToken: stoppingToken
        );
    }

    private async Task HandleOrderCreatedMessageAsync(string message, CancellationToken token)
    {
        using var scope = _serviceProvider.CreateScope();
        var inventoryService = scope.ServiceProvider.GetRequiredService<IInventoryService>();

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
                    OrderItems =
                    [
                        new OrderItem
                        {
                            ProductId = item.Product.ProductId,
                            Quantity = item.Quantity,
                            UnitPrice = item.Product.Price,
                            TotalPrice = item.TotalPrice,
                            WarehouseId = warehouseId
                        }
                    ]
                };

                var messageContent = JsonSerializer.Serialize(order);

                var deliveryResult = await _producerService.ProduceAsync(Topic_Inventory_Reserved,order.User.UserId.ToString() ,messageContent, token);

                if (deliveryResult != null && deliveryResult.Status == PersistenceStatus.Persisted)
                {
                    _logger.LogInformation("Order for ProductId: {ProductId} successfully sent to topic {Topic} with WarehouseId: {WarehouseId}",
                        item.Product.ProductId, Topic_Inventory_Reserved, warehouseId);
                }
                else
                {
                    _logger.LogWarning("Failed to send order for ProductId: {ProductId} to topic {Topic}. Result: {Result}",
                        item.Product.ProductId, Topic_Inventory_Reserved, deliveryResult?.Status);
                }

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
