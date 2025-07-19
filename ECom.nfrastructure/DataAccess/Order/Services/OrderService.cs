using ECom.Infrastructure.DataAccess.Order.Repositories;
using Ecommerce.Common.Models.Orders;

namespace ECom.Infrastructure.DataAccess.Order.Services;

public class OrderService(IOrderRepository repository) : IOrderService
{
    public async Task<OrderDto> GetOrderAsync(Guid orderId, CancellationToken token)
    {
        return await repository.GetOrder(orderId, token);
    }

    public async Task UpdateOrderStatusAsync(Guid orderId,string status ,CancellationToken cancellationToken)
    {
        await repository.UpdateOrderStatus(orderId, status,cancellationToken);
    }

    public async Task<Guid> CreateOrderAsync(Ecommerce.Common.Models.Order? order, CancellationToken cancellationToken)
    {
        return await repository.InsertFullOrderAsync(order!, cancellationToken);
    }
}
