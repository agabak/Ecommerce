using Microsoft.Data.SqlClient;
using System.Data;

namespace Ecom_InventoryWorkerService.Databases;

public class InventoryConnectionProvider
{
    private readonly IConfiguration _config;
    public InventoryConnectionProvider(IConfiguration config) => _config = config;
    public IDbConnection GetConnection() =>
        new SqlConnection(_config.GetConnectionString("InventoryConnection"));
}
