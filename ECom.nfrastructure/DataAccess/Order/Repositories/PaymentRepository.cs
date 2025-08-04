using Dapper;
using System.Data;

namespace ECom.Infrastructure.DataAccess.Order.Repositories;

public class PaymentRepository:DataAccessProvider, IPaymentRepository
{
    public PaymentRepository(string connectionString):base(connectionString)
    {
    }

    public async Task UpdatePaymentStatus(Guid orderId, string status, CancellationToken token)
    {
        using var _db = GetOpenConnection();
        try
        {
            var rowsAffected = await _db.ExecuteAsync(
                @"UPDATE Payments SET Status = @Status, ProcessedDate = SYSDATETIME() WHERE OrderId = @OrderId",
                new { OrderId = orderId, Status = status });
            if (rowsAffected == 0)
            {
                throw new KeyNotFoundException($"Payment with ID {orderId} not found.");
            }
        }
        catch { throw; }
    }

}
