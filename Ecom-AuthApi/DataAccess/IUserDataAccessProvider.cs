using System.Data;

namespace Ecom_AuthApi.DataAccess;

public interface IUserDataAccessProvider
{
    IDbConnection DbConnection();
}
