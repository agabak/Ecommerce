using Ecommerce.Common.Models;

namespace Ecom_OderWorkerService.Services;

public interface IOrderService
{
    Task CreateOrderAsync(Order? order, CancellationToken cancellationToken);
}