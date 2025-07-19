using Confluent.Kafka;
using ECom.Infrastructure.DataAccess.Order.Services;
using Ecommerce.Common.Models;
using Ecommerce.Common.Services.Kafka;
using System.Text.Json;

namespace Ecom_OderWorkerService;

public class CreateOrderBackgroundService(
    IConsumerService consumer,
    IProducerService producerService,
    ILogger<CreateOrderBackgroundService> logger,
    IServiceProvider serviceProvider) : BackgroundService
{
    private const string TopicOrder_Inventory_Reserved = "Inventory.Reserved";
    private const string TopicPayment_Completed = "Payment.Completed";
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("CreateOrderBackgroundService started. Subscribing to topic: {Topic}", TopicOrder_Inventory_Reserved);

        await ProcessCreateOrder(stoppingToken);

        logger.LogInformation("CreateOrderBackgroundService is running.");
    }

    private async Task ProcessCreateOrder(CancellationToken cancellationToken)
    {
        await consumer.ProcessAsync(
            topic: TopicOrder_Inventory_Reserved,
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

            var orderId =  await orderService.CreateOrderAsync(order, cancellationToken);

             if (orderId != Guid.Empty)  
             {
               var deliveryResult =   await producerService.ProduceAsync(
                      topic: TopicPayment_Completed,
                      key:order?.User?.UserId.ToString() ?? "UserId",
                      message: orderId.ToString(), 
                      cancellationToken: cancellationToken
                  );

                if (deliveryResult != null && deliveryResult.Status == PersistenceStatus.Persisted) 
                { 
                  logger.LogInformation("Order creation message successfully produced to topic {Topic} with key {Key} and value {Value}",
                      TopicPayment_Completed, order?.User?.UserId.ToString() ?? "UserId", orderId);
                }
             }

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

