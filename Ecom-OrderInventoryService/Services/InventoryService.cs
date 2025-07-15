using Ecom_OrderInventoryService.Repositories;
using Ecommerce.Common.Models;

namespace Ecom_OrderInventoryService.Services;

public class InventoryService(IInventoryRepository repository) : IInventoryService
{
    public async Task<Dictionary<Guid, Guid>> UpdateInventoryAfterOrderAsync(List<Item> items, CancellationToken token)
    {
        return await repository.UpdateInventoryAfterOrderAsync(items, token);
    }
}
