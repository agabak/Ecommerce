using Ecommerce.Common.Models.Users;

namespace ECom.Infrastructure.DataAccess.User.Services;

public interface IUserService
{
    Task<UserDto> CreateUser(CreateUserDto model, CancellationToken token = default);
    Task<UpdateUserDto> UpdateUser(Guid userId, UpdateUserDto model, CancellationToken token = default);

    Task<bool> DeleteUser(Guid userId, CancellationToken token = default);

    Task<UserWithAddressDto> GetUserWithAddressById(string userName, CancellationToken token = default);

    Task<bool> IsUserUniqueAsync(string username, string email, CancellationToken token = default);
}
