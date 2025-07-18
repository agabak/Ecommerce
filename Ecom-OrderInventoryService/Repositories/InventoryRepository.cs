using Dapper;
using Ecommerce.Common.DataAccess;
using Ecommerce.Common.Models;
using System.Data;

namespace Ecom_OrderInventoryService.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly IDbConnection _db;
    private readonly IDataAccessProvider _provider;

    public InventoryRepository(IDataAccessProvider provider)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        _db = _provider.CreateDbConnection();
    }

    public async Task<Dictionary<Guid, Guid>> UpdateInventoryAfterOrderAsync(List<Item> items, CancellationToken token)
    {
        var result = new Dictionary<Guid, Guid>();
        EnsureOpen(token);
        try
        {
            foreach (var item in items)
            {
                // 1. Get current inventory for the product, including WarehouseId
                const string selectSql = "SELECT Quantity, WarehouseId FROM dbo.Inventory WHERE ProductId = @ProductId";
                var inventory = await _db.QueryFirstOrDefaultAsync<(int Quantity, Guid WarehouseId)>(
                    selectSql, new { item.Product.ProductId });

                if (inventory.Equals(default((int, Guid))))
                {
                    // Could not find inventory for this product, skip or throw (your choice)
                    continue;
                }

                int finalQty;
                int buffer = 100;
                int ordered = item.Quantity;

                if (inventory.Quantity < ordered)
                {
                    int toAdd = (ordered - inventory.Quantity) + buffer;
                    int newQty = inventory.Quantity + toAdd;
                    finalQty = newQty - ordered;
                }
                else
                {
                    finalQty = inventory.Quantity - ordered;
                }

                const string updateSql = @"
                UPDATE dbo.Inventory
                SET Quantity = @FinalQty,
                    LastUpdated = SYSDATETIME()
                WHERE ProductId = @ProductId;";

                int rowsAffected = await _db.ExecuteAsync(
                    new CommandDefinition(updateSql, new { FinalQty = finalQty, item.Product.ProductId }, cancellationToken: token)
                );

                if (rowsAffected > 0)
                    result[item.Product.ProductId] = inventory.WarehouseId;
                // If update failed, do not add to result
            }
            return result;
        }
        catch (Exception ex)
        {
            // Optional: log the exception here if you want
            // _logger.LogError(ex, "Inventory update failed.");
            return new Dictionary<Guid, Guid>(); // Return empty on failure
        }
    }

    private void EnsureOpen(CancellationToken ct)
    {
        if (_db.State != ConnectionState.Open)
            _db.Open();
    }
}


