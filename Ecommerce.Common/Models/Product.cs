namespace Ecommerce.Common.Models;

public class Product
{
    public Guid ProductId { get; set; }
    public decimal Price { get; set; }
    public string ProductName { get; set; } = string.Empty;
}
