using Ecommerce.Common.Models;

namespace Ecom_InventoryWorkerService.Services
{
    public interface IInventoryService
    {
        Task<Dictionary<Guid, Guid>> UpdateInventoryAfterOrderAsync(List<Item> items, CancellationToken token);
        Task EnsureInventoryRecordAsync(Guid productId, CancellationToken token = default);
    }
}