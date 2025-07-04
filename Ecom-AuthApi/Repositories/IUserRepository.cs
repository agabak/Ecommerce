using Ecom_AuthApi.Model.Dtos;
using Ecommerce.Common.Models.Users;

namespace Ecom_AuthApi.Repositories
{
    public interface IUserRepository
    {
        Task<UserDto> CreateUser(CreateUserDto user, CancellationToken token = default);
        Task<UpdateUserDto> UpdateUser(User user, CancellationToken token = default);

        Task<User> GetUserById(Guid userId, CancellationToken token = default);

        Task<bool> DeleteUser(Guid userId, CancellationToken token = default);

        Task<UserWithAddressDto> GetUserWithAddressById(string userName, CancellationToken token = default);

        Task<bool> IsUserUniqueAsync(string username, string email, CancellationToken token = default);
    }
}
