using Ecommerce.Common.Models.Orders;

namespace Ecommerce.Common.Notifications.Email;

public static class OrderNotificationEmailBuilder
{
    public static string BuildOrderNotificationEmailBody(OrderDto notification, string supportEmail = "support@yourcompany.com")
    {
        if (notification == null) throw new ArgumentNullException(nameof(notification));

        var emailBody = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset=""UTF-8"">
                <title>Order Update</title>
            </head>
            <body style=""font-family: Arial, sans-serif;"">
                <h2>Thank you for your payment, {notification.FullName}!</h2>
                <p>
                    We have received your payment for your order, and your order status is now <strong>{notification.OrderStatus}</strong>.
                </p>
                <table style=""margin: 16px 0;"">
                    <tr>
                        <td><strong>Order ID:</strong></td>
                        <td>{notification.OrderId}</td>
                    </tr>
                    <tr>
                        <td><strong>Payment Status:</strong></td>
                        <td>{notification.PaymentStatus}</td>
                    </tr>
                    <tr>
                        <td><strong>Shipping Address:</strong></td>
                        <td>{notification.Address}</td>
                    </tr>
                </table>
                <p>
                    If you have any questions, please contact us at 
                    <a href=""mailto:{supportEmail}"">{supportEmail}</a>.
                </p>
            </body>
            </html>";
        return emailBody;
    }
}
