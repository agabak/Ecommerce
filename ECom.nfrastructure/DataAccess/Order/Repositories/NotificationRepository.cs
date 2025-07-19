using Dapper;
using System.Data;

namespace ECom.Infrastructure.DataAccess.Order.Repositories;
public class NotificationRepository : INotificationRepository
{
    private readonly IDbConnection _db;
    private readonly IDataAccessProvider _provider;

    public NotificationRepository(IDataAccessProvider provider)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        _db = _provider.CreateDbConnection() ?? throw new ArgumentNullException(nameof(_provider));
    }

    public async Task AddNotificationAsync(Guid userId, Guid sourceId, string type, string source, string message, CancellationToken cancellationToken = default)
    {
        var sql = @"
                INSERT INTO Notifications
                    (UserId, Title, Message, Type, Status, SourceId, SourceType)
                VALUES
                    (@UserId, @Title, @Message, @Type, @Status, @SourceId, @SourceType)";

        EnsureOpen(cancellationToken);

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

    private void EnsureOpen(CancellationToken token)
    {
        if (_db.State != ConnectionState.Open)
        {
            _db.Open();
        }
    }
}
