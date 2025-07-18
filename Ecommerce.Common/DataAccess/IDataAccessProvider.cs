using Microsoft.Data.SqlClient;
using System.Data;

namespace Ecommerce.Common.DataAccess;

public interface IDataAccessProvider 
{
    IDbConnection CreateDbConnection();
}
