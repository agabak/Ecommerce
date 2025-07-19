using Microsoft.Data.SqlClient;
using System.Data;

namespace ECom.Infrastructure.DataAccess.Order;

public class OrderDataAccessProvider(string connectionString) : IDataAccessProvider
{
    public IDbConnection CreateDbConnection()
    {
        return new SqlConnection(connectionString);
    }
}
