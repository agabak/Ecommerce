using Ecom_NotificationWorkerService.Models;
using Ecom_NotificationWorkerService.Repositories;

namespace Ecom_NotificationWorkerService.Services;

public class NotificationService(INotificationRepository repository) : INotificationService
{
    public async Task<OrderNotification> GetOrderNotificationAsync(Guid orderId, CancellationToken token)
    {
        return await repository.GetOrderNotificationAsync(orderId, token);
    }

    public async Task SendNotificationAsync(OrderNotification notification, CancellationToken stoppingToken)
    {
        var notificationMessage = OrderNotificationEmailBuilder.BuildOrderNotificationEmailBody(notification);

        await repository.SendNotificationAsync(notification, notificationMessage, stoppingToken);
    }
}