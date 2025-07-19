using Ecommerce.Common.Models.Users;

namespace Ecom_AuthApi.Services
{
    public interface IJwtTokenService
    {
        string GenerateToken(UserWithAddressDto user);
    }
}
