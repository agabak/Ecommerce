namespace Ecommerce.Common.Services.Kafka;

public interface IConsumerService
{
    Task ProcessAsync(string topic, Func<string, Task> messageHandler, CancellationToken cancellationToken = default);
}