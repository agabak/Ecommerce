using Microsoft.Data.SqlClient;
using System.Data;

namespace Ecom_InventoryWorkerService.Databases;

public class InventoryConnectionProvider : IInventoryConnectionProvider
{
    private readonly string _connectionString;
    public InventoryConnectionProvider(string connectionString) => _connectionString = connectionString;
    public IDbConnection DbConnection() => new SqlConnection(_connectionString);
}
