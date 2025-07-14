using Ecom_AuthApi.Model.Dtos;
using Ecom_AuthApi.Repositories;

namespace Ecom_AuthApi.Services
{
    public sealed class UserService(IUserRepository repository) : IUserService
    {
        public async Task<UserDto> CreateUser(CreateUserDto model, CancellationToken token = default)
        {
            ValidateCreateUserModel(model);
            model.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

            return await repository.CreateUser(model, token);
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

        private static void ValidateCreateUserModel(CreateUserDto model)
        {
            if (string.IsNullOrWhiteSpace(model.Username))
                throw new ArgumentException("Username is required.", nameof(model.Username));
            if (string.IsNullOrWhiteSpace(model.Email))
                throw new ArgumentException("Email is required.", nameof(model.Email));
            if (string.IsNullOrWhiteSpace(model.Password))
                throw new ArgumentException("Password is required.", nameof(model.Password));
        }
    }
}
