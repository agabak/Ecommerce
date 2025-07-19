using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECom.Infrastructure.DataAccess.Product;

public class ProductDataAccessProvider(string connectionString) : IDataAccessProvider
{
    public IDbConnection CreateDbConnection()
    {
        return new SqlConnection(connectionString);
    }
}
