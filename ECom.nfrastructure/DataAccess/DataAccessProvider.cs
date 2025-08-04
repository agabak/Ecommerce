using System.Data;
using Microsoft.Data.SqlClient;

namespace ECom.Infrastructure.DataAccess
{
    public abstract class DataAccessProvider : IDataAccessProvider
    {
        private readonly string _connectionString;

        protected DataAccessProvider(string connectionString)
        {
            _connectionString = connectionString
                ?? throw new ArgumentNullException(nameof(connectionString));
        }

        // Protected so inheritors can use
        protected virtual  IDbConnection CreateDbConnection()
        {
            return new SqlConnection(_connectionString);
        }

        // This returns an open connection to the caller
        public virtual IDbConnection GetOpenConnection()
        {
            var connection = CreateDbConnection();
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            return connection;
        }
    }
}
