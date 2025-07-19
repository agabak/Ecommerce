using Ecommerce.Common.DataAccess.Inventory.Repositories;
using Ecommerce.Common.Models;

namespace Ecommerce.Common.DataAccess.Inventory.Services;

public class InventoryService(IInventoryRepository repository) : IInventoryService
{
    public async Task<Dictionary<Guid, Guid>> UpdateInventoryAfterOrderAsync(List<Item> items, CancellationToken token)
    {
        return await repository.UpdateInventoryAfterOrderAsync(items, token);
    }

    public async Task EnsureInventoryRecordAsync(Guid productId, CancellationToken token = default)
    {
        await repository.EnsureInventoryRecordAsync(productId, token);
    }
}

