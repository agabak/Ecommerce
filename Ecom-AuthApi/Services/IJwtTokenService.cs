using Ecom_AuthApi.Model.Dtos;

namespace Ecom_AuthApi.Services
{
    public interface IJwtTokenService
    {
        string GenerateToken(UserWithAddressDto user);
    }
}
