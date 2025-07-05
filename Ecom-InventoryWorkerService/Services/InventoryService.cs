
using Ecom_InventoryWorkerService.Repositories;

namespace Ecom_InventoryWorkerService.Services
{
    public class InventoryService(IInventoryRepository repository) : IInventoryService
    {
        public async Task UpsertInventoryAsync(Guid productId, CancellationToken token = default)
        {
           await repository.UpsertInventoryAsync(productId, token);
        }
    }
}
