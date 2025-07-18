using System.Data;

namespace Ecom_NotificationWorkerService.DataAccess;

public interface IOrderDataAccessProvider
{
    IDbConnection CreateDbConnection();
}