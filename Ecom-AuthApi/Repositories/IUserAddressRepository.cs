using Ecommerce.Common.Models.Users;

namespace Ecom_AuthApi.Repositories
{
    public interface IUserAddressRepository
    {
        Task<UserAddress> CreateUserAddress(UserAddress userAddress,
            CancellationToken token = default);

        Task<UserAddress> UpdateUserAddress(UserAddress userAddress, 
            CancellationToken token = default);

        Task<UserAddress> DeleteUserAddress(Guid addressId, CancellationToken token = default);

        Task<UserAddress> GetUserAddress(Guid addressId, CancellationToken token = default);
    }
}
