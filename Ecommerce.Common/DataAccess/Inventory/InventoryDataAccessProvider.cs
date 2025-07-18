using Microsoft.Data.SqlClient;
using System.Data;

namespace Ecommerce.Common.DataAccess.Inventory
{
    public class InventoryDataAccessProvider(string connectionString) : IDataAccessProvider
    {
        public IDbConnection CreateDbConnection()
        {
           return new SqlConnection(connectionString);
        }
    }
}
