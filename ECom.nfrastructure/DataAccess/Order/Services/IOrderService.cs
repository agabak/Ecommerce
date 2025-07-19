using Ecommerce.Common.Models.Orders;

namespace ECom.Infrastructure.DataAccess.Order.Services;

public interface IOrderService
{
    Task<Guid> CreateOrderAsync(Ecommerce.Common.Models.Order? order, CancellationToken cancellationToken);
    Task ProcessStatusAsync(Guid orderId, CancellationToken cancellationToken);
    Task<OrderNotification> GetOrderNotificationAsync(Guid orderId, CancellationToken token);
    Task SendNotificationAsync(OrderNotification notification, CancellationToken stoppingToken);
}