using System.Data;

namespace ECom.Infrastructure.DataAccess
{
    public interface IDataAccessProvider
    {
        IDbConnection CreateDbConnection();

        public void EnsureConnection(IDbConnection connection)
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
        }
    }
}
