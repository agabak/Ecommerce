using Ecommerce.Common.Models;

namespace Ecom_Client.Services
{
    public interface ICartService
    {
        List<Item> CartItems { get; }

        event Action? OnChange;

        void AddToCart(Product product, int quantity);
        void Clear();
        void UpdateQuantity(Guid productId, int quantity);
        void RemoveFromCart(Guid productId);
    }
}