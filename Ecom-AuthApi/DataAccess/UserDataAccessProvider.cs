using Microsoft.Data.SqlClient;
using System.Data;

namespace Ecom_AuthApi.DataAccess;

public class UserDataAccessProvider : IUserDataAccessProvider
{
    private readonly string _connectionString;
    public UserDataAccessProvider(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection DbConnection()
    {
        return  new SqlConnection(_connectionString);
    }
}
