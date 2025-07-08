using Confluent.Kafka;
using Ecommerce.Common.Services.Kafka;

namespace Ecom_ProductApi.Services;

public class KafkaProducerService : IKafkaProducerService
{
    private readonly IProducerService _producer;
    private readonly ILogger<KafkaProducerService> _logger;

    public KafkaProducerService(IProducerService producer, ILogger<KafkaProducerService> logger)
    {
        _producer = producer;
        _logger = logger;
    }

    public async Task ProduceAsync(string topic, string message, CancellationToken cancellationToken = default)
    {
        await _producer.ProduceAsync(topic, message, cancellationToken);
    }
}

