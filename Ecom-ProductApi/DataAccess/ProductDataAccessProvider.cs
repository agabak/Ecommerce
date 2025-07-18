using Microsoft.Data.SqlClient;
using System.Data;

namespace Ecom_ProductApi.DataAccess;

public class ProductDataAccessProvider : IProductDataAccessProvider
{
    private readonly string _connectionString;
    public ProductDataAccessProvider(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public IDbConnection DbConnection()
    {
        return new SqlConnection(_connectionString);
    }
}
