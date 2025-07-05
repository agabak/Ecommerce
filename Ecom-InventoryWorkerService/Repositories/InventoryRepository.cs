using Dapper;
using System.Data;

namespace Ecom_InventoryWorkerService.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly IDbConnection _db;

    public InventoryRepository(IDbConnection db)
    {
        _db = db;
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

