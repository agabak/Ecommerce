namespace ECom.Infrastructure.DataAccess.Order.Repositories;

public interface INotificationRepository
{
    Task AddNotificationAsync(Guid userId, Guid sourceId, string type, string source, string message, CancellationToken cancellationToken = default);
}
