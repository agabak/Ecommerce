using Ecommerce.Common.Models.Orders;

namespace ECom.Infrastructure.DataAccess.Order.Repositories
{
    public interface IOrderRepository
    {
        Task<Guid> InsertFullOrderAsync(Ecommerce.Common.Models.Order order, CancellationToken token);
        Task UpdateOrderStatus(Guid orderId, string status, CancellationToken token);
        Task UpdatePaymentStatus(Guid orderId, string status, CancellationToken token);
        Task<OrderNotification> GetOrderNotificationAsync(Guid orderId, CancellationToken token);
        Task SendNotificationAsync(OrderNotification notification, string notificationMessage, CancellationToken stoppingToken);
    }
}