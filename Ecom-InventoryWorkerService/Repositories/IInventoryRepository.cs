
namespace Ecom_InventoryWorkerService.Repositories
{
    public interface IInventoryRepository
    {
        Task UpsertInventoryAsync(Guid productId, CancellationToken token = default);
    }
}