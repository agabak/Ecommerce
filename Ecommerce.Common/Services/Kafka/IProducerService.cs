namespace Ecommerce.Common.Services.Kafka
{
    public interface IProducerService
    {
        Task ProduceAsync(string topic, string message, CancellationToken cancellationToken = default);
    }
}