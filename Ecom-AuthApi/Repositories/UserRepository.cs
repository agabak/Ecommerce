using Dapper;
using Ecom_AuthApi.Model.Dtos;
using Ecommerce.Common.Models.Users;
using System.Data;

namespace Ecom_AuthApi.Repositories
{
    public class UserRepository(IDbConnection db) : IUserRepository
    {
        public async Task<UserDto> CreateUser(CreateUserDto dto, CancellationToken token = default)
        {
            EnsureOpen(token);

            using var tran = db.BeginTransaction();
            try
            {
                var addressInsertSql = @"
                INSERT INTO UserAddress (Street, City, State, ZipCode,Created)
                OUTPUT INSERTED.AddressId
                VALUES (@Street, @City, @State, @ZipCode, GETDATE());
                ";
                var addressId = await db.QuerySingleAsync<Guid>(
                    addressInsertSql,
                    new
                    {
                        dto.Street,
                        dto.City,
                        dto.State,
                        dto.ZipCode
                    },
                    transaction: tran
                );

                var userInsertSql = @"
                INSERT INTO Users (AddressId, Username, Email, PasswordHash, FirstName, LastName, Phone)
                OUTPUT INSERTED.UserId
                VALUES (@AddressId, @Username, @Email, @PasswordHash, @FirstName, @LastName, @Phone);
                ";
                var userId = await db.QuerySingleAsync<Guid>(
                    userInsertSql,
                    new
                    {
                        AddressId = addressId,
                        dto.Username,
                        dto.Email,
                        dto.PasswordHash,
                        dto.FirstName,
                        dto.LastName,
                        dto.Phone
                    },
                    transaction: tran
                );

                tran.Commit();

                return new UserDto(userId, addressId, dto.Username, dto.Email, dto.FirstName, dto.LastName, dto.Phone);
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        public async Task<bool> DeleteUser(Guid userId, CancellationToken token = default)
        {
            EnsureOpen(token);

            using var tran = db.BeginTransaction();
            try
            {
                var affectedRows = await db.ExecuteAsync(
                    "DELETE FROM Users WHERE UserId = @UserId",
                    new { UserId = userId },
                    transaction: tran
                );

                tran.Commit();
                // affectedRows will be 1 if deleted, 0 if not found
                return affectedRows > 0;
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        public async Task<UserWithAddressDto?> GetUserWithAddressById(string userName, CancellationToken token = default)
        {
            var sql = @"
            select UserId, UserName, Email, FirstName, LastName,PasswordHash, Phone, Street, City, State, ZipCode from Users u
            join UserAddress ad on u.AddressId = ad.AddressId
            where u.UserName = @UserName;
            ";
            EnsureOpen(token);
            var user = await db.QuerySingleOrDefaultAsync<UserWithAddressDto>(
                sql,
                new { UserName = userName },
                commandType: CommandType.Text
            );
            return user;
        }

        public async Task<UpdateUserDto> UpdateUser(User user, CancellationToken token = default)
        {
            var sql = @"
            UPDATE Users
            SET Username = @Username, 
                Email = @Email, 
                FirstName = @FirstName,    
                LastName = @LastName, 
                Phone = @Phone
            WHERE UserId = @UserId;";

            EnsureOpen(token);

            var affected = await db.ExecuteAsync(
                sql,
                new
                {
                    user.UserId,
                    user.Username,
                    user.Email,
                    user.FirstName,
                    user.LastName,
                    user.Phone
                },
                commandType: CommandType.Text
            );

            if (affected > 0)
            {
                return new UpdateUserDto(user.Username,user.Email, user.FirstName, user.LastName, user.Phone); // Or fetch updated user from DB if you want the latest data
            }
            else
            {
                throw new KeyNotFoundException("User not found");
            }
        }

        public async Task<bool> IsUserUniqueAsync(string username, string email, CancellationToken token = default)
        {
            const string sql = @"
            SELECT 
                CASE 
                    WHEN EXISTS (SELECT 1 FROM Users WHERE Username = @Username OR Email = @Email)
                    THEN CAST(0 AS BIT)
                    ELSE CAST(1 AS BIT)
                END AS IsUnique;";

            return await db.QuerySingleAsync<bool>(
                sql,
                new { Username = username, Email = email },
                commandType: CommandType.Text
            );
        }

        public async Task<User> GetUserById(Guid userId, CancellationToken token = default)
        {
            const string sql = @"
                SELECT UserId, Username, Email, FirstName, LastName, Phone 
                FROM Users 
                WHERE UserId = @UserId;";

            EnsureOpen(token);

            var user = await db.QuerySingleOrDefaultAsync<User>(
                sql,
                new { UserId = userId },
                commandType: CommandType.Text
            );

            if (user == null)
                throw new KeyNotFoundException($"User with ID '{userId}' does not exist.");

            return user;
        }

        private void EnsureOpen(CancellationToken ct)
        {
            if (db.State != ConnectionState.Open)
                db.Open();
        }


    }
}
