using Dapper;
using Ecommerce.Common.Models;
using Ecommerce.Common.Models.Orders;
using System.Data;

namespace ECom.Infrastructure.DataAccess.Order.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly IDbConnection _db;
        private readonly IDataAccessProvider _provider;

        public OrderRepository(IDataAccessProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _db = _provider.CreateDbConnection() ?? throw new ArgumentNullException(nameof(provider));
        }

        public async Task UpdateOrderStatus(Guid orderId, string status, CancellationToken token)
        {
            EnsureOpen(token);
            try
            {
                var rowsAffected = await _db.ExecuteAsync(
                @"UPDATE Orders SET Status = @Status, UpdatedAt = SYSDATETIME() WHERE OrderId = @OrderId",
                new { OrderId = orderId, Status = status });
            }
            catch
            {
                throw;
            }
        }

        public async Task<OrderNotification> GetOrderNotificationAsync(Guid orderId, CancellationToken token)
        {
            const string query = @"
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
                  AND od.OrderId = @OrderId"; ;
            EnsureOpen(token);
            return await _db.QueryFirstOrDefaultAsync<OrderNotification>(query, new { OrderId = orderId });
        }

        // I will like to separate the notification sending logic from the repository
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
            }
            catch { throw; }
        }

        public async Task<Guid> InsertFullOrderAsync(Ecommerce.Common.Models.Order order, CancellationToken token)
        {
            var (street, city, state, zip) = SplitAddress(order?.User?.Address!);

            EnsureOpen(token);
            using var tran = _db.BeginTransaction();
            try
            {
                // 1. Insert or get User
                var user = order?.User!;
                var userId = await _db.ExecuteScalarAsync<Guid?>(
                    @"SELECT TOP 1 UserId FROM Users WHERE Email = @Email",
                    new { user.Email }, tran) ?? Guid.NewGuid();

                if (userId == Guid.Empty)
                    userId = Guid.NewGuid();

                await _db.ExecuteAsync(@"
                IF NOT EXISTS (SELECT 1 FROM Users WHERE UserId = @UserId)
                    INSERT INTO Users (UserId, Email, FullName, CreatedAt)
                    VALUES (@UserId, @Email, @FullName, SYSDATETIME())",
                    new
                    {
                        UserId = userId,
                        user.Email,
                        FullName = user.Name
                    }, tran);

                // 2. Get or insert/update UserAddress
                var addressId = await GetOrUpdateUserAddressAsync(userId, street, city, state, zip, tran, token);

                // 3. Insert Order
                var orderId = Guid.NewGuid();
                await _db.ExecuteAsync(@"
                INSERT INTO Orders (
                    OrderId, UserId, OrderDate, Status, TotalAmount, PaymentId, TrackingNumber, UpdatedAt, AddressId
                ) VALUES (
                    @OrderId, @UserId, @OrderDate, @Status, @TotalAmount, NULL, @TrackingNumber, @UpdatedAt, @AddressId
                )",
                    new
                    {
                        OrderId = orderId,
                        UserId = userId,
                        order.OrderDate,
                        order.Status,
                        order.TotalAmount,
                        order.TrackingNumber,
                        order.UpdatedAt,
                        AddressId = addressId
                    }, tran);

                // 4. Get or insert/update Payment
                var paymentId = await GetOrUpdatePaymentAsync(
                    orderId,
                    PaymentTypeExtensions.GetDisplayName(order.PaymentType),
                    order.TotalAmount,
                    "In Process",
                    tran,
                    token
                );

                // 5. Update Order with PaymentId
                await _db.ExecuteAsync(@"
                UPDATE Orders SET PaymentId = @PaymentId WHERE OrderId = @OrderId",
                    new { PaymentId = paymentId, OrderId = orderId }, tran);

                // 6. Insert OrderItems
                foreach (var item in order.OrderItems)
                {
                    await _db.ExecuteAsync(@"
                    INSERT INTO OrderItems (
                        OrderItemId, OrderId, ProductId, WarehouseId, Quantity, UnitPrice, TotalPrice
                    ) VALUES (
                        @OrderItemId, @OrderId, @ProductId, @WarehouseId, @Quantity, @UnitPrice, @TotalPrice
                    )",
                        new
                        {
                            OrderItemId = Guid.NewGuid(),
                            OrderId = orderId,
                            item.ProductId,
                            item.WarehouseId,
                            item.Quantity,
                            item.UnitPrice,
                            item.TotalPrice
                        }, tran);
                }

                tran.Commit();
                return orderId;
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        private async Task<Guid> GetOrUpdateUserAddressAsync(
            Guid userId, string street, string city, string state, string zip, IDbTransaction tran, CancellationToken token)
        {
            EnsureOpen(token);
            // Try to find an existing current address for the user
            var address = await _db.QueryFirstOrDefaultAsync<dynamic>(@"
            SELECT TOP 1 AddressId, Street, City, State, ZipCode
            FROM UserAddress
            WHERE UserId = @UserId AND ISCurrent = 1
            ORDER BY CreatedAt DESC",
                new { UserId = userId }, tran);

            if (address != null)
            {
                // Check if the fields are all equal (case-insensitive compare)
                bool changed = !string.Equals((string)address.Street, street, StringComparison.OrdinalIgnoreCase)
                            || !string.Equals((string)address.City, city, StringComparison.OrdinalIgnoreCase)
                            || !string.Equals((string)address.State, state, StringComparison.OrdinalIgnoreCase)
                            || !string.Equals((string)address.ZipCode, zip, StringComparison.OrdinalIgnoreCase);

                if (!changed)
                {
                    // No change—return the current AddressId
                    return (Guid)address.AddressId;
                }
                else
                {
                    // Mark old as not current
                    await _db.ExecuteAsync(
                        @"UPDATE UserAddress SET ISCurrent = 0, UpdatedAt = SYSDATETIME() WHERE AddressId = @AddressId",
                        new { AddressId = (Guid)address.AddressId }, tran);
                    // Insert new address
                    var newAddressId = Guid.NewGuid();
                    await _db.ExecuteAsync(@"
                    INSERT INTO UserAddress (
                        AddressId, UserId, Street, City, State, ZipCode, Country, ISCurrent, CreatedAt
                    ) VALUES (
                        @AddressId, @UserId, @Street, @City, @State, @ZipCode, @Country, 1, SYSDATETIME()
                    )",
                        new
                        {
                            AddressId = newAddressId,
                            UserId = userId,
                            Street = street,
                            City = city,
                            State = state,
                            ZipCode = zip,
                            Country = "USA"
                        }, tran);
                    return newAddressId;
                }
            }
            else
            {
                // No current address—insert new
                var addressId = Guid.NewGuid();
                await _db.ExecuteAsync(@"
                INSERT INTO UserAddress (
                    AddressId, UserId, Street, City, State, ZipCode, Country, ISCurrent, CreatedAt
                ) VALUES (
                    @AddressId, @UserId, @Street, @City, @State, @ZipCode, @Country, 1, SYSDATETIME()
                )",
                    new
                    {
                        AddressId = addressId,
                        UserId = userId,
                        Street = street,
                        City = city,
                        State = state,
                        ZipCode = zip,
                        Country = "USA"
                    }, tran);
                return addressId;
            }
        }

        private async Task<Guid> GetOrUpdatePaymentAsync(
            Guid orderId, string paymentMethod, decimal amount, string status, IDbTransaction tran, CancellationToken token)
        {
            EnsureOpen(token);
            // Check for existing payment for this order with the same method and amount
            var payment = await _db.QueryFirstOrDefaultAsync<dynamic>(@"
            SELECT PaymentId, PaymentMethod, Amount, Status
            FROM Payments
            WHERE OrderId = @OrderId AND PaymentMethod = @PaymentMethod AND Amount = @Amount",
                new { OrderId = orderId, PaymentMethod = paymentMethod, Amount = amount }, tran);

            if (payment != null)
            {
                // If status has changed, update it
                if (!string.Equals((string)payment.Status, status, StringComparison.OrdinalIgnoreCase))
                {
                    await _db.ExecuteAsync(@"
                    UPDATE Payments SET Status = @Status, TransactionDate = SYSDATETIME()
                    WHERE PaymentId = @PaymentId",
                        new { Status = status, PaymentId = (Guid)payment.PaymentId }, tran);
                }
                return (Guid)payment.PaymentId;
            }
            else
            {
                // Insert new payment
                var paymentId = Guid.NewGuid();
                await _db.ExecuteAsync(@"
                INSERT INTO Payments (
                    PaymentId, OrderId, PaymentMethod, Amount, Status, TransactionDate, PaymentReference
                ) VALUES (
                    @PaymentId, @OrderId, @PaymentMethod, @Amount, @Status, SYSDATETIME(), NEWID()
                )",
                    new
                    {
                        PaymentId = paymentId,
                        OrderId = orderId,
                        PaymentMethod = paymentMethod,
                        Amount = amount,
                        Status = status
                    }, tran);
                return paymentId;
            }
        }

        private static (string Street, string City, string State, string ZipCode) SplitAddress(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                return ("", "", "", "");

            var parts = address.Split(',', StringSplitOptions.RemoveEmptyEntries)
                               .Select(x => x.Trim()).ToArray();

            return (
                parts.Length > 0 ? parts[0] : "",
                parts.Length > 1 ? parts[1] : "",
                parts.Length > 2 ? parts[2] : "",
                parts.Length > 3 ? parts[3] : ""
            );
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
