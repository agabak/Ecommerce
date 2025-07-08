using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Common.Services.Kafka;

public class ProducerService : IProducerService
{
    private readonly IProducer<Null, string> _producer;
    private readonly ILogger<ProducerService> _logger;

    public ProducerService(IProducer<Null, string> producer, ILogger<ProducerService> logger)
    {
        _producer = producer;
        _logger = logger;
    }

    public async Task ProduceAsync(string topic, string message, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _producer.ProduceAsync(
                topic,
                new Message<Null, string> { Value = message },
                cancellationToken
            );
            _logger.LogInformation("Delivered message to {TopicPartitionOffset}", result.TopicPartitionOffset);
        }
        catch (ProduceException<Null, string> ex)
        {
            _logger.LogError(ex, "Kafka delivery failed to topic {Topic}: {Reason}", topic, ex.Error.Reason);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while producing message to topic {Topic}", topic);
            throw;
        }
    }
}
