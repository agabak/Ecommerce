namespace Ecommerce.Common.Models.Inventory;

public class WarehouseDto
{
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
}

public class InventoryDto
{
    public Guid InventoryId { get; set; }
    public Guid ProductId { get; set; }      // Comes from Product service
    public Guid WarehouseId { get; set; }
    public int Quantity { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class InventoryTransactionDto
{
    public Guid TransactionId { get; set; }
    public Guid ProductId { get; set; }
    public Guid? WarehouseId { get; set; }
    public int QuantityChange { get; set; }
    public string TransactionType { get; set; } = string.Empty; // "Sale", "Restock", etc.
    public DateTime TransactionDate { get; set; }
    public Guid? OrderId { get; set; } // Comes from Order service
    public string Notes { get; set; } = string.Empty;
}

