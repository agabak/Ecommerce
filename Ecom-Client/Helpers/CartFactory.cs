using Ecommerce.Common.Models;

namespace Ecom_Client.Helpers;

public static class CartFactory
{
    public static Cart CreateCartItems(User user, List<Item> items)
    {
        return new Cart
        {
            User = user,
            Items = items
        };
    }
}
