namespace Ecommerce.Common.Models.Orders;

public class UserDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class OrderDto
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; } 
    public string ShippingAddress { get; set; } = string.Empty;
    public Guid? PaymentId { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
}

public class OrderItemDto
{
    public Guid OrderItemId { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }  // Comes from Product service
    public Guid? WarehouseId { get; set; } // Comes from Inventory service
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

public class PaymentDto
{
    public Guid PaymentId { get; set; }
    public Guid OrderId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string PaymentReference { get; set; } = string.Empty;
}

public class ShoppingCartDto
{
    public Guid CartId { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ShoppingCartItemDto
{
    public Guid CartItemId { get; set; }
    public Guid CartId { get; set; }
    public Guid ProductId { get; set; }  // Comes from Product service
    public int Quantity { get; set; }
    public DateTime AddedAt { get; set; }
}
