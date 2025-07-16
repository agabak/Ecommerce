using Ecom_NotificationWorkerService.Models;

namespace Ecom_NotificationWorkerService.Services;

public interface INotificationService
{
    Task<OrderNotification> GetOrderNotificationAsync(Guid orderId, CancellationToken token);
    Task SendNotificationAsync(OrderNotification notification, CancellationToken stoppingToken);
}