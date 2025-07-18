using System.Data;

namespace Ecom_ProductApi.DataAccess
{
    public interface IProductDataAccessProvider
    {
        IDbConnection DbConnection();
    }
}