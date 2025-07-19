using Ecommerce.Common.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECom.Infrastructure.DataAccess.User.Repositories;

public interface IUserRepository
{
    Task<UserDto> CreateUser(CreateUserDto user, CancellationToken token = default);
    Task<UpdateUserDto> UpdateUser(Ecommerce.Common.Models.Users.User user, CancellationToken token = default);

    Task<Ecommerce.Common.Models.Users.User> GetUserById(Guid userId, CancellationToken token = default);

    Task<bool> DeleteUser(Guid userId, CancellationToken token = default);

    Task<UserWithAddressDto?> GetUserWithAddressById(string userName, CancellationToken token = default);

    Task<bool> IsUserUniqueAsync(string username, string email, CancellationToken token = default);
}
