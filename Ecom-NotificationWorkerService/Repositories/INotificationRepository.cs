using Ecom_NotificationWorkerService.Models;

namespace Ecom_NotificationWorkerService.Repositories;

public interface INotificationRepository
{
    Task<OrderNotification> GetOrderNotificationAsync(Guid orderId, CancellationToken token);
    Task SendNotificationAsync(OrderNotification notification, string notificationMessage, CancellationToken stoppingToken);
}