using Microsoft.Data.SqlClient;
using System.Data;

namespace Ecom_NotificationWorkerService.DataAccess;

public class OrderDataAccessProvider : IOrderDataAccessProvider
{
    private readonly string _connectionString;

    public OrderDataAccessProvider(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public IDbConnection CreateDbConnection()
    {
       return new SqlConnection(_connectionString); 
    }
}
