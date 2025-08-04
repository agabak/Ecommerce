using Dapper;
using Ecommerce.Common.Models.Users;
using System.Data;
using System.Text;
using System.Collections.Generic;
using System.Reflection.Metadata;

namespace ECom.Infrastructure.DataAccess.User.Repositories
{
    public class UserMySqlRepository : UserMySqlDataAccessProvider, IUserRepository
    {
        public UserMySqlRepository(string connectionString) : base(connectionString)
        {
        }

        public async Task<UserDto> CreateUser(CreateUserDto dto, CancellationToken token = default)
        {
            using var db = GetOpenConnection();
            using var tran = db.BeginTransaction();

            try
            {
                // 1. Insert Address
                var addressId = Guid.NewGuid().ToString();
                var addressInsertSql = @"
                    INSERT INTO userAddress (AddressId, Street, City, State, ZipCode, Created)
                    VALUES (@AddressId, @Street, @City, @State, @ZipCode, NOW());
                ";
                await db.ExecuteAsync(
                    addressInsertSql,
                    new
                    {
                        AddressId = addressId,
                        dto.Street,
                        dto.City,
                        dto.State,
                        dto.ZipCode
                    },
                    transaction: tran
                );

                // 2. Insert User
                var userId = Guid.NewGuid().ToString();
                var userInsertSql = @"
                    INSERT INTO users (UserId, AddressId, Username, Email, PasswordHash, FirstName, LastName, Phone)
                    VALUES (@UserId, @AddressId, @Username, @Email, @PasswordHash, @FirstName, @LastName, @Phone);
                ";
                await db.ExecuteAsync(
                    userInsertSql,
                    new
                    {
                        UserId = userId,
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

                // 3. Insert UserRoles
                if (dto.Roles?.Any() == true)
                {
                    // Get RoleIds by role name
                    var roleQuery = "SELECT RoleId, Name FROM roles WHERE Name IN @RoleNames";
                    var roles = (await db.QueryAsync<(Guid RoleId, string Name)>(
                        roleQuery,
                        new { RoleNames = dto.Roles },
                        transaction: tran
                    )).ToList();

                    var roleInsertSql = @"
                        INSERT INTO UserRoles (UserId, RoleId, AssignedAt)
                        VALUES (@UserId, @RoleId, NOW());
                    ";

                    foreach (var role in roles)
                    {
                        await db.ExecuteAsync(
                            roleInsertSql,
                            new { UserId = userId, RoleId = role.RoleId },
                            transaction: tran
                        );
                    }
                }

                tran.Commit();

                return new UserDto(Guid.Parse(userId), Guid.Parse(addressId), dto.Username, dto.Email, dto.FirstName, dto.LastName, dto.Phone);
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        public async Task<bool> DeleteUser(Guid userId, CancellationToken token = default)
        {
            using var db = GetOpenConnection();
            using var tran = db.BeginTransaction();
            try
            {
                var affectedRows = await db.ExecuteAsync(
                    "DELETE FROM Users WHERE UserId = @UserId",
                    new { UserId = userId.ToString() },
                    transaction: tran
                );

                tran.Commit();
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
            const string sql = @"
                SELECT 
                    u.UserId, u.Username, u.Email, u.FirstName, u.LastName, u.PasswordHash, u.Phone,
                    ad.Street, ad.City, ad.State, ad.ZipCode,
                    r.Name AS RoleName
                FROM Users u
                JOIN UserAddress ad ON u.AddressId = ad.AddressId
                LEFT JOIN UserRoles ur ON u.UserId = ur.UserId
                LEFT JOIN Roles r ON ur.RoleId = r.RoleId
                WHERE u.Username = @UserName;
            ";

            using var db = GetOpenConnection();
            var userDictionary = new Dictionary<string, UserWithAddressDto>();

            var result = await db.QueryAsync<UserWithAddressDto, string?, UserWithAddressDto>(
                sql,
                (user, role) =>
                {
                    if (!userDictionary.TryGetValue(user.UserId.ToString(), out var userEntry))
                    {
                        userEntry = user;
                        userEntry.Roles = new List<string>();
                        userDictionary.Add(user.UserId.ToString(), userEntry);
                    }

                    if (!string.IsNullOrEmpty(role) && !userEntry.Roles.Contains(role))
                    {
                        userEntry.Roles.Add(role);
                    }

                    return userEntry;
                },
                new { UserName = userName },
                splitOn: "RoleName"
            );

            return userDictionary.Values.FirstOrDefault();
        }

        public async Task<UpdateUserDto> UpdateUser(Ecommerce.Common.Models.Users.User user, CancellationToken token = default)
        {
            var sql = @"
                UPDATE Users
                SET Username = @Username, 
                    Email = @Email, 
                    FirstName = @FirstName,    
                    LastName = @LastName, 
                    Phone = @Phone
                WHERE UserId = @UserId;
            ";

            using var db = GetOpenConnection();
            var affected = await db.ExecuteAsync(
                sql,
                new
                {
                    UserId = user.UserId.ToString(),
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
                return new UpdateUserDto(user.Username, user.Email, user.FirstName, user.LastName, user.Phone);
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
                        THEN 0
                        ELSE 1
                    END AS IsUnique;
            ";
            using var db = GetOpenConnection();
            return await db.QuerySingleAsync<bool>(
                sql,
                new { Username = username, Email = email },
                commandType: CommandType.Text
            );
        }

        public async Task<Ecommerce.Common.Models.Users.User> GetUserById(Guid userId, CancellationToken token = default)
        {
            const string sql = @"
                SELECT UserId, Username, Email, FirstName, LastName, Phone 
                FROM Users 
                WHERE UserId = @UserId;
            ";

            using var db = GetOpenConnection();
            var user = await db.QuerySingleOrDefaultAsync<Ecommerce.Common.Models.Users.User>(
                sql,
                new { UserId = userId.ToString() },
                commandType: CommandType.Text
            );

            if (user == null)
                throw new KeyNotFoundException($"User with ID '{userId}' does not exist.");

            return user;
        }
    }
}

