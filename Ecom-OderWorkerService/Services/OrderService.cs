using Ecom_OderWorkerService.Repositories;
using Ecommerce.Common.Models;

namespace Ecom_OderWorkerService.Services;

public class OrderService(IOrderRepository repository) : IOrderService
{
    public async Task CreateOrderAsync(Order? order, CancellationToken cancellationToken)
    {
       await repository.InsertFullOrderAsync(order!, cancellationToken);
    }
}
