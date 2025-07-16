using Ecom_PaymentWorkerService.Repositories;

namespace Ecom_PaymentWorkerService.Services;

public class PaymentService(IPaymentRepository _payment,
    IOrderRepository _order) : IPaymentService
{
    public async Task ProcessStatusAsync(Guid orderId, CancellationToken cancellationToken)
    {
        await _payment.UpdatePaymentStatus(orderId, "Processed", cancellationToken);

        await _order.UpdateOrderStatus(orderId, "Paid", cancellationToken);
    }
}
