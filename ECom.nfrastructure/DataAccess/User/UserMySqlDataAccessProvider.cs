using MySql.Data.MySqlClient;
using System.Data;

namespace ECom.Infrastructure.DataAccess.User;

public abstract class UserMySqlDataAccessProvider: DataAccessProvider
{
    private readonly string _connectionString;
    public UserMySqlDataAccessProvider(string connectionString) : base(connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    protected override IDbConnection CreateDbConnection()
    {
        return new MySqlConnection(_connectionString);
    }
}
