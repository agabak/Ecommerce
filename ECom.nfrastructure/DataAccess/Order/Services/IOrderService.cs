using Ecommerce.Common.Models.Orders;

namespace ECom.Infrastructure.DataAccess.Order.Services;

public interface IOrderService
{
    Task<Guid> CreateOrderAsync(Ecommerce.Common.Models.Order? order, CancellationToken cancellationToken);
    Task UpdateOrderStatusAsync(Guid orderId,string status, CancellationToken cancellationToken);
    Task<OrderDto> GetOrderAsync(Guid orderId, CancellationToken token);
}