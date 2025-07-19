using ECom.Infrastructure.DataAccess.Order.Services;
using Ecommerce.Common.Notifications.Email;
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

        if (!Guid.TryParse(message?.Trim(), out var orderId))
        {
            logger.LogWarning("Invalid order ID received: {Message}", message);
            return;
        }

        using var scope = provider.CreateScope();
        var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var order = await orderService.GetOrderAsync(orderId, stoppingToken);

        if (order == null)
        {
            logger.LogWarning("No notification found for OrderId: {OrderId}", orderId);
            return;
        }

        var messageEmail = OrderNotificationEmailBuilder.BuildOrderNotificationEmailBody(order);
        await notificationService.AddNotificationAsync(order.UserId, orderId, "Order", "Order", messageEmail, stoppingToken);
    }
}
