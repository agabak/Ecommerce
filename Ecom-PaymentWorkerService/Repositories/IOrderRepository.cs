namespace Ecom_PaymentWorkerService.Repositories;

public interface IOrderRepository
{
    Task UpdateOrderStatus(Guid orderId, string status, CancellationToken token);
}