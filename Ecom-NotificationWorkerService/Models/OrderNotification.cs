namespace Ecom_NotificationWorkerService.Models;

public class OrderNotification
{
    public Guid UserId { get; set; }
    public Guid OrderId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string OrderStatus { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}

