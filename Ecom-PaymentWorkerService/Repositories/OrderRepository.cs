using System.Data;
using Dapper;

namespace Ecom_PaymentWorkerService.Repositories;

public class OrderRepository(IDbConnection _db) : IOrderRepository
{
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
