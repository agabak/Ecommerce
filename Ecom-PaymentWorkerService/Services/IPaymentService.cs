
namespace Ecom_PaymentWorkerService.Services;

public interface IPaymentService
{
    Task ProcessStatusAsync(Guid orderId, CancellationToken cancellationToken);
}