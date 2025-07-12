using Ecom_AuthApi.Model.Dtos;
using Ecom_AuthApi.Repositories;

namespace Ecom_AuthApi.Services
{
    public sealed class UserService(IUserRepository repository) : IUserService
    {
        public async Task<UserDto> CreateUser(UserAddressDto model, CancellationToken token = default)
        {
           var userDto = new CreateUserDto
            {
                Username = model.Username,
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                FirstName = model.FirstName,
                LastName = model.LastName,
                Phone = model.Phone,
                Street = model.Street,
                City = model.City,
                State = model.State,
                ZipCode = model.ZipCode
            };
            return await repository.CreateUser(userDto, token);
        }

        public async Task<bool> DeleteUser(Guid userId, CancellationToken token = default)
        {
            return await repository.DeleteUser(userId, token);
        }

        public async Task<UserWithAddressDto> GetUserWithAddressById(string userName, CancellationToken token = default)
        {
           return await repository.GetUserWithAddressById(userName, token);
        }

        public async Task<UpdateUserDto> UpdateUser(Guid userId, UpdateUserDto model, CancellationToken token = default)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty.", nameof(userId));

            var user = await repository.GetUserById(userId, token);

            if (user == null)
                throw new KeyNotFoundException($"User with ID '{userId}' does not exist.");

            // Update relevant fields
            user.Username = model.Username ?? user.Username;
            user.Email = model.Email ?? user.Email;
            user.FirstName = model.FirstName ?? user.FirstName;
            user.LastName = model.LastName ?? user.LastName;
            user.Phone = model.Phone ?? user.Phone;

            return await repository.UpdateUser(user, token);
        }

        public async Task<bool> IsUserUniqueAsync(string username, string email, CancellationToken token = default)
        {
            return await repository.IsUserUniqueAsync(username, email, token);
        }
    }
}
