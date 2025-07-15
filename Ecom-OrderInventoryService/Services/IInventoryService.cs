using Ecommerce.Common.Models;

namespace Ecom_OrderInventoryService.Services;

public interface IInventoryService
{
    Task<Dictionary<Guid, Guid>> UpdateInventoryAfterOrderAsync(List<Item> items, CancellationToken token);
}
