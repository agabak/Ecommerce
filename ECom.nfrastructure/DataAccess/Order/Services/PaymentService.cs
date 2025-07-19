using ECom.Infrastructure.DataAccess.Order.Repositories;

namespace ECom.Infrastructure.DataAccess.Order.Services;

public class PaymentService(IPaymentRepository repository) : IPaymentService
{
    public async Task UpdatePaymentStatus(Guid orderId, string status, CancellationToken token)
    {
        await repository.UpdatePaymentStatus(orderId, status, token);
    }
}
