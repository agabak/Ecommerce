using Dapper;

namespace ECom.Infrastructure.DataAccess.Order.Repositories;
public class NotificationRepository :DataAccessProvider, INotificationRepository
{
    public NotificationRepository(string connectionString):base(connectionString)
    {
    }

    public async Task AddNotificationAsync(Guid userId, Guid sourceId, string type, string source, string message, CancellationToken cancellationToken = default)
    {
        var sql = @"
                INSERT INTO Notifications
                    (UserId, Title, Message, Type, Status, SourceId, SourceType)
                VALUES
                    (@UserId, @Title, @Message, @Type, @Status, @SourceId, @SourceType)";


        using var _db = GetOpenConnection();
        var rowsAffected = await _db.ExecuteAsync(sql, new
        {
            UserId = userId,
            Title = "Order Confirmation",
            Message = message,
            Type = type,
            Status = "Sent",
            SourceId = sourceId,
            SourceType = source
        });

        if (rowsAffected == 0)
        {
            throw new Exception("Failed to insert notification.");
        }
    }
}
