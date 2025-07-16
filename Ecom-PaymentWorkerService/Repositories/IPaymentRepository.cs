namespace Ecom_PaymentWorkerService.Repositories;

public interface IPaymentRepository
{
    Task UpdatePaymentStatus(Guid orderId, string status, CancellationToken token);
}