using System.Data;
using Dapper;
using Ecommerce.Common.DataAccess;

namespace Ecom_PaymentWorkerService.Repositories;

public class PaymentRepository: IPaymentRepository
{
    private readonly IDbConnection _db;
    private readonly IDataAccessProvider _provider;
    public PaymentRepository(IDataAccessProvider provider)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        _db = _provider.CreateDbConnection() ?? throw new ArgumentNullException(nameof(provider));
    }
    public async Task UpdatePaymentStatus(Guid orderId, string status, CancellationToken token)
    {
        EnsureOpen(token);
        try
        {
            var rowsAffected = await _db.ExecuteAsync(
                @"UPDATE Payments SET Status = @Status, ProcessedDate = SYSDATETIME() WHERE OrderId = @OrderId",
                new { OrderId = orderId, Status = status });
            if (rowsAffected == 0)
            {
                throw new KeyNotFoundException($"Payment with ID {orderId} not found.");
            }
        }catch{ throw; }
    }

    private void EnsureOpen(CancellationToken token)
    {
        if (_db.State != ConnectionState.Open)
        {
            _db.Open();
        }
    }
}
