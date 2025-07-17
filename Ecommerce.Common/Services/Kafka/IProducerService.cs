namespace Ecommerce.Common.Services.Kafka
{
    public interface IProducerService
    {
        Task ProduceAsync(string topic,string key, string message, CancellationToken cancellationToken = default);
    }
}