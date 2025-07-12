namespace Ecommerce.Common.Models;

public class Item
{
    public User? User { get; set; }
    public Product Product { get; set; } = default!;
    public int Quantity { get; set; }
    public decimal TotalPrice => Product.Price * Quantity;
}
