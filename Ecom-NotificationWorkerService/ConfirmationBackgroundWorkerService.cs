using ECom.Infrastructure.DataAccess.Order.Services;
using Ecommerce.Common.Services.Kafka;

namespace Ecom_NotificationWorkerService;

public class ConfirmationBackgroundWorkerService(
    IConsumerService consumer,
    ILogger<ConfirmationBackgroundWorkerService> logger,
    IServiceProvider provider)
    : BackgroundService
{
    private const string TopicOrder_Notification = "Order.Notification";
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
       // Log the start of the service
        logger.LogInformation("ConfirmationBackgroundWorkerService started. Subscribing to topic: {Topic}", TopicOrder_Notification);
        // Start processing messages from the topic
        return consumer.ProcessAsync(
            topic: TopicOrder_Notification,
            messageHandler: async (message) =>
            {
                await HandleOrderNotificationMessageAsync(message, stoppingToken);
            },
            cancellationToken: stoppingToken
        );
    }

    private async Task HandleOrderNotificationMessageAsync(string message, CancellationToken stoppingToken)
    {
        logger.LogInformation("Received order notification message: {Message}", message);
        using var scope = provider.CreateScope();
        var notificationService = scope.ServiceProvider.GetRequiredService<IOrderService>();

        if (!Guid.TryParse(message?.Trim(), out var orderId)) return;
        
            var notification = await notificationService.GetOrderNotificationAsync(orderId, stoppingToken);

            if (notification == null)
            {
                logger.LogWarning("No notification found for OrderId: {OrderId}", orderId);
                return;
            }

            await notificationService.SendNotificationAsync(notification, stoppingToken);
        
    }
}
