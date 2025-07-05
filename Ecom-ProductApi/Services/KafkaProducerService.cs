using Confluent.Kafka;

namespace Ecom_ProductApi.Services;

public class KafkaProducerService : IKafkaProducerService
{
    private readonly IProducer<Null, string> _producer;
    private readonly ILogger<KafkaProducerService> _logger;

    public KafkaProducerService(IProducer<Null, string> producer, ILogger<KafkaProducerService> logger)
    {
        _producer = producer;
        _logger = logger;
    }

    public async Task ProduceAsync(string topic, string message, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _producer.ProduceAsync(topic, new Message<Null, string> { Value = message }, cancellationToken);
            _logger.LogInformation("Delivered message to {TopicPartitionOffset}", result.TopicPartitionOffset);
        }
        catch (ProduceException<Null, string> ex)
        {
            _logger.LogError(ex, "Kafka delivery failed: {Reason}", ex.Error.Reason);
            throw;
        }
    }
}

