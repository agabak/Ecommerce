using Ecommerce.Common.Models;

namespace Ecom_Client.Services;

public class CartService : ICartService
{
    public List<Item> CartItems { get; } = new();

    public event Action? OnChange;

    public void AddToCart(Product product, int quantity)
    {
        var item = CartItems.FirstOrDefault(x => x.Product.ProductId == product.ProductId);
        if (item != null)
            item.Quantity += quantity;
        else
            CartItems.Add(new Item { Product = product, Quantity = quantity });

        OnChange?.Invoke();
    }

    public void UpdateQuantity(Guid productId, int quantity)
    {
        var item = CartItems.FirstOrDefault(x => x.Product.ProductId == productId);
        if (item != null)
        {
            if (quantity <= 0)
            {
                CartItems.Remove(item); // Remove from cart if quantity is zero or less
            }
            else
            {
                item.Quantity = quantity;
            }
            OnChange?.Invoke();
        }
    }


    public void RemoveFromCart(Guid productId)
    {
        var item = CartItems.FirstOrDefault(x => x.Product.ProductId == productId);
        if (item != null)
            CartItems.Remove(item);

        OnChange?.Invoke();
    }

    public void Clear() => CartItems.Clear();
}




