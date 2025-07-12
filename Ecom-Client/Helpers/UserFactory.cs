using Ecom_Client.Models;
using Ecommerce.Common.Models;

namespace Ecom_Client.Helpers;

public static class UserFactory
{
    public static User CreateJwtUser(JwtUser jwt)
    {
        return new User
        {
            UserId = jwt.UserId,
            Email = jwt.Email,
            Name = jwt.Name,
            Address = jwt.Address,
        };
    }
}

