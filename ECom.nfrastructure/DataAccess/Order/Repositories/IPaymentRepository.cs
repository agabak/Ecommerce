namespace ECom.Infrastructure.DataAccess.Order.Repositories;

public interface IPaymentRepository
{
    Task UpdatePaymentStatus(Guid orderId, string status, CancellationToken token);
}
