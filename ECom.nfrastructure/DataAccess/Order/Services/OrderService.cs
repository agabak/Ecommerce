using ECom.Infrastructure.DataAccess.Order.Repositories;
using Ecommerce.Common.Models.Orders;
using Ecommerce.Common.Notifications.Email;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECom.Infrastructure.DataAccess.Order.Services;

public class OrderService(IOrderRepository repository) : IOrderService
{
    public async Task<OrderNotification> GetOrderNotificationAsync(Guid orderId, CancellationToken token)
    {
        return await repository.GetOrderNotificationAsync(orderId, token);
    }

    public async Task ProcessStatusAsync(Guid orderId, CancellationToken cancellationToken)
    {
        await repository.UpdatePaymentStatus(orderId,"Processed", cancellationToken);
        await repository.UpdateOrderStatus(orderId, "Paid",cancellationToken);
    }

    public async Task SendNotificationAsync(OrderNotification notification, CancellationToken stoppingToken)
    {
        var notificationMessage = OrderNotificationEmailBuilder.BuildOrderNotificationEmailBody(notification);

        await repository.SendNotificationAsync(notification, notificationMessage, stoppingToken);
    }

    public async Task UpdateOrderStatus(Guid orderId, string status, CancellationToken token)
    {
         await repository.UpdateOrderStatus(orderId,status,token);
    }

    public async Task<Guid> CreateOrderAsync(Ecommerce.Common.Models.Order? order, CancellationToken cancellationToken)
    {
        return await repository.InsertFullOrderAsync(order!, cancellationToken);
    }
}
