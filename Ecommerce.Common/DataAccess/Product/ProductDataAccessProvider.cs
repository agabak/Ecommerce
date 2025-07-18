using Microsoft.Data.SqlClient;
using System.Data;

namespace Ecommerce.Common.DataAccess.Product;

public class ProductDataAccessProvider(string connectionString) : IDataAccessProvider
{
    public IDbConnection CreateDbConnection()
    {
        return new SqlConnection(connectionString);
    }
}
