using System.Data;

namespace ECom.Infrastructure.DataAccess
{
    public interface IDataAccessProvider
    {
        IDbConnection CreateDbConnection();
    }
}
