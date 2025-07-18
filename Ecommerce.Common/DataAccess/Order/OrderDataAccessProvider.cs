using Microsoft.Data.SqlClient;
using System.Data;

namespace Ecommerce.Common.DataAccess.Order
{
    public class OrderDataAccessProvider(string connectionString) : IDataAccessProvider
    {
        public IDbConnection CreateDbConnection()
        {
            return new SqlConnection(connectionString);
        }
    }
}
