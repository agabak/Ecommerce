using System.Data;

namespace Ecom_InventoryWorkerService.Databases
{
    public interface IInventoryConnectionProvider
    {
        IDbConnection DbConnection();
    }
}