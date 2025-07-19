using Ecommerce.Common.Models;

namespace Ecommerce.Common.DataAccess.Inventory.Services;

public interface IInventoryService
{
    Task<Dictionary<Guid, Guid>> UpdateInventoryAfterOrderAsync(List<Item> items, CancellationToken token);
    Task EnsureInventoryRecordAsync(Guid productId, CancellationToken token = default);
}
