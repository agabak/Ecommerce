using Ecommerce.Common.Models;

namespace Ecommerce.Common.DataAccess.Inventory.Repositories;

public interface IInventoryRepository
{
    Task<Dictionary<Guid, Guid>> UpdateInventoryAfterOrderAsync(List<Item> items, CancellationToken token);
    Task EnsureInventoryRecordAsync(Guid productId, CancellationToken token = default);
}
