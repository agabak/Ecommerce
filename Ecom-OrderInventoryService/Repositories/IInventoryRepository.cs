using Ecommerce.Common.Models;

namespace Ecom_OrderInventoryService.Repositories;

public interface IInventoryRepository
{
    Task<Dictionary<Guid, Guid>> UpdateInventoryAfterOrderAsync(List<Item> items, CancellationToken token);
}
