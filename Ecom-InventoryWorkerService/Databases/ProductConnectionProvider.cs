using Microsoft.Data.SqlClient;
using System.Data;

namespace Ecom_InventoryWorkerService.Databases;

public class ProductConnectionProvider
{
    private readonly IConfiguration _config;
    public ProductConnectionProvider(IConfiguration config) => _config = config;
    public IDbConnection GetConnection() =>
        new SqlConnection(_config.GetConnectionString("OrderConnection"));
}