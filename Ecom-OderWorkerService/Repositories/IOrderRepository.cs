using Ecommerce.Common.Models;

namespace Ecom_OderWorkerService.Repositories
{
    public interface IOrderRepository
    {
        Task<Guid> InsertFullOrderAsync(Order order, CancellationToken token);
    }
}