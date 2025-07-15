
using Ecom_InventoryWorkerService.Repositories;
using Ecommerce.Common.Models;

namespace Ecom_InventoryWorkerService.Services
{
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
}
