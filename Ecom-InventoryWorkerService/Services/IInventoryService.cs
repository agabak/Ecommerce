namespace Ecom_InventoryWorkerService.Services
{
    public interface IInventoryService
    {
        Task UpsertInventoryAsync(Guid productId, CancellationToken token = default);
    }
}