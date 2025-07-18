using Dapper;
using Ecom_NotificationWorkerService.Models;
using Ecommerce.Common.DataAccess;
using System.Data;

namespace Ecom_NotificationWorkerService.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly IDbConnection _db;
        private readonly IDataAccessProvider _provider;

        public NotificationRepository(IDataAccessProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _db = provider.CreateDbConnection();
        }

        public async Task<OrderNotification> GetOrderNotificationAsync(Guid orderId, CancellationToken token)
        {
            EnsureOpen(token);

            var query = @"
                SELECT
                    od.UserId,
                    od.OrderId,
                    us.FullName,
                    us.Email,
                    py.Status AS PaymentStatus,
                    od.Status AS OrderStatus,
                    ISNULL(ur.Street, '') + ' ' +
                    ISNULL(ur.City, '') + ' ' +
                    ISNULL(ur.State, '') + ' ' +
                    ISNULL(ur.Country, '') + ' ' +
                    ISNULL(ur.ZipCode, '') AS Address
                FROM Payments py
                JOIN Orders od ON py.OrderId = od.OrderId
                JOIN UserAddress ur ON od.AddressId = ur.AddressId
                JOIN Users us ON us.UserId = od.UserId
                WHERE py.Status = 'Processed'
                  AND od.Status = 'Paid'
                  AND od.OrderId = @OrderId";

            return await _db.QueryFirstOrDefaultAsync<OrderNotification>(query, new { OrderId = orderId });
        }

        public async Task SendNotificationAsync(OrderNotification notification, string notificationMessage, CancellationToken stoppingToken)
        {
            var sql = @"
                INSERT INTO Notifications
                    (UserId, Title, Message, Type, Status, SourceId, SourceType)
                VALUES
                    (@UserId, @Title, @Message, @Type, @Status, @SourceId, @SourceType)";

            EnsureOpen(stoppingToken);

            var rowsAffected = await _db.ExecuteAsync(sql, new
            {
                UserId = notification.UserId,
                Title = "Order Confirmation",
                Message = notificationMessage,
                Type = "Order",
                Status = "Sent",
                SourceId = notification.OrderId,
                SourceType = "Order"
            });

            if (rowsAffected == 0)
            {
                throw new Exception("Failed to insert notification.");
            }
        }

        private void EnsureOpen(CancellationToken token)
        {
            if (_db.State != ConnectionState.Open)
            {
                _db.Open();
            }
        }
    }
}

