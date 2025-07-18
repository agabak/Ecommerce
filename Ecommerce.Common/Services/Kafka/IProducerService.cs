using Confluent.Kafka;

namespace Ecommerce.Common.Services.Kafka
{
    public interface IProducerService
    {
        Task<DeliveryResult<string, string>> ProduceAsync(string topic,string key, string message, CancellationToken cancellationToken = default);
    }
}