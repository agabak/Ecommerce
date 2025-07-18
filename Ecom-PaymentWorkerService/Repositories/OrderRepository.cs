using System.Data;
using Dapper;
using Ecommerce.Common.DataAccess;

namespace Ecom_PaymentWorkerService.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly IDbConnection _db;
    private readonly IDataAccessProvider _provider;
    public OrderRepository(IDataAccessProvider provider)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        _db = provider?.CreateDbConnection() ?? throw new ArgumentNullException(nameof(provider));
    }
    
    public async Task UpdateOrderStatus(Guid orderId, string status, CancellationToken token)
    {
        EnsureOpen(token);
        try
        {
            var rowsAffected = await _db.ExecuteAsync(
            @"UPDATE Orders SET Status = @Status, UpdatedAt = SYSDATETIME() WHERE OrderId = @OrderId",
            new { OrderId = orderId, Status = status });
        }
        catch
        {
            throw;
        }
    }
    private void EnsureOpen(CancellationToken token)
    {
        if (_db.State != ConnectionState.Open)
        {
            _db.Open();
        }
    }


}
