using Dapper;
using Ecommerce.Common.Models;
using System.Data;

namespace Ecom_InventoryWorkerService.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly IDbConnection _db;

    public InventoryRepository(IDbConnection db)
    {
        _db = db;
    }

    public async Task<Dictionary<Guid, Guid>> UpdateInventoryAfterOrderAsync(List<Item> items, CancellationToken token)
    {
        var result = new Dictionary<Guid, Guid>();

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

    public async Task UpsertInventoryAsync(Guid productId, CancellationToken token = default)
    {
        const string selectInventorySql = @"
        SELECT InventoryId, Quantity
        FROM dbo.Inventory
        WHERE ProductId = @ProductId;";

        var inventory = await _db.QueryFirstOrDefaultAsync<(Guid InventoryId, int Quantity)>(
            new CommandDefinition(selectInventorySql, new { ProductId = productId }, cancellationToken: token)
        );

        if (inventory != default)
        {
            // Inventory exists – update quantity
            const string updateSql = @"
            UPDATE dbo.Inventory
            SET Quantity = Quantity + 1,
                LastUpdated = SYSDATETIME()
            WHERE ProductId = @ProductId;";

            await _db.ExecuteAsync(
                new CommandDefinition(updateSql, new { ProductId = productId }, cancellationToken: token)
            );
        }
        else
        {
            // Inventory doesn't exist – pick a random warehouse and insert
            const string selectWarehouseSql = @"
            SELECT TOP 1 WarehouseId 
            FROM dbo.Warehouses 
            ORDER BY NEWID();";

            var warehouseId = await _db.ExecuteScalarAsync<Guid>(
                new CommandDefinition(selectWarehouseSql, cancellationToken: token)
            );

            const string insertSql = @"
            INSERT INTO dbo.Inventory (ProductId, WarehouseId, Quantity, LastUpdated)
            VALUES (@ProductId, @WarehouseId, 100, SYSDATETIME());";

            await _db.ExecuteAsync(
                new CommandDefinition(insertSql, new { ProductId = productId, WarehouseId = warehouseId }, cancellationToken: token)
            );
        }
    }
}

