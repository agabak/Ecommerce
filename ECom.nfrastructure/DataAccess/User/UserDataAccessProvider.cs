using Microsoft.Data.SqlClient;
using System.Data;

namespace ECom.Infrastructure.DataAccess.User;

public class UserDataAccessProvider(string connectionString) : IDataAccessProvider
{
    public IDbConnection CreateDbConnection()
    {
        return new SqlConnection(connectionString);
    }
}
