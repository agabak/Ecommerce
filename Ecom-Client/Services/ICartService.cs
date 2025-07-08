using Ecom_Client.Models.DTOs;

namespace Ecom_Client.Services
{
    public interface ICartService
    {
        List<CartItem> CartItems { get; }

        event Action? OnChange;

        void AddToCart(ProductDto product, int quantity);
        void Clear();
        void UpdateQuantity(Guid productId, int quantity);
        void RemoveFromCart(Guid productId);
    }
}