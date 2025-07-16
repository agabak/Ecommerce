using Ecom_OderWorkerService.Repositories;
using Ecommerce.Common.Models;

namespace Ecom_OderWorkerService.Services;

public class OrderService(IOrderRepository repository) : IOrderService
{
    public async Task<Guid> CreateOrderAsync(Order? order, CancellationToken cancellationToken)
    {
      return  await repository.InsertFullOrderAsync(order!, cancellationToken);
    }
}
