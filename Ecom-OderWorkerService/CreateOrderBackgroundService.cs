using Ecom_OderWorkerService.Services;
using Ecommerce.Common.Models;
using Ecommerce.Common.Services.Kafka;
using System.Text.Json;

namespace Ecom_OderWorkerService;

public class CreateOrderBackgroundService(
    IConsumerService consumer,
    ILogger<CreateOrderBackgroundService> logger,
    IServiceProvider serviceProvider) : BackgroundService
{
    private const string TopicOrderInventoryCreated = "Order-Inventory-Created";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("CreateOrderBackgroundService started. Subscribing to topic: {Topic}", TopicOrderInventoryCreated);

        await ProcessCreateOrder(stoppingToken);

        logger.LogInformation("CreateOrderBackgroundService is running.");
    }

    private async Task ProcessCreateOrder(CancellationToken cancellationToken)
    {
        await consumer.ProcessAsync(
            topic: TopicOrderInventoryCreated,
            messageHandler: async (message) =>
            {
                await HandleCreateOrderMessageAsync(message, cancellationToken);
            },
            cancellationToken: cancellationToken
        );
    }

    private async Task HandleCreateOrderMessageAsync(string message, CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

        try
        {
            logger.LogInformation("Received order creation message: {Message}", message);

            var order = JsonSerializer.Deserialize<Order>(message);
            if (order == null)
            {
                logger.LogWarning("Deserialized order is null. Raw message: {Message}", message);
                return;
            }

            await orderService.CreateOrderAsync(order, cancellationToken);

            logger.LogInformation("Successfully processed order creation for User: {User}, OrderItems: {Count}",
                order.User?.Email ?? "Unknown", order.OrderItems.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process order creation message. Raw message: {Message}", message);
            // Optionally: don't rethrow, just log and continue to avoid crashing background service
            // throw;
        }
    }
}

