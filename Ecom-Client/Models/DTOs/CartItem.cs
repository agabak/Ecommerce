namespace Ecom_Client.Models.DTOs;

public class CartItem
{
    public ProductDto Product { get; set; } = default!;
    public int Quantity { get; set; }
}

