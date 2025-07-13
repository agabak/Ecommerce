namespace Ecommerce.Common.Models;

public class Item
{
    public Product Product { get; set; } = default!;
    public int Quantity { get; set; }
    public decimal TotalPrice => Product.Price * Quantity;
}
