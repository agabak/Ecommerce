using Microsoft.Data.SqlClient;
using System.Data;

namespace Ecommerce.Common.DataAccess.User;

public class UserDataAccessProvider(string connectionString) : IDataAccessProvider
{
    public IDbConnection CreateDbConnection()
    {
        return new SqlConnection(connectionString);
    }
}
