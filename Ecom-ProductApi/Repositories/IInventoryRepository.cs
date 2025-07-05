namespace Ecom_ProductApi.Repositories;

public interface IInventoryRepository
{
    Task MergeInventory(List<Guid> productIds, CancellationToken token = default);
}

