using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Common.Services.Kafka;

public class ProducerService(IProducer<string, string> _producer,
    ILogger<ProducerService> _logger) : IProducerService
{

    public async Task<DeliveryResult<string, string>> ProduceAsync(string topic,string key, string message, CancellationToken cancellationToken = default)
    {

        try
        {
           var result = await _producer.ProduceAsync(
                topic,
                new Message<string, string> {Key = key, Value = message },
                cancellationToken
            );

            if(result.Status == PersistenceStatus.Persisted)
            {
                _logger.LogInformation("Message produced to topic {Topic} at partition {Partition}, offset {Offset}",
                    topic, result.Partition, result.Offset);
            }
            else
            {
                _logger.LogWarning("Message produced to topic {Topic} but not persisted. Status: {Status}",
                    topic, result.Status);
            }

            return result;
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(ex, "Kafka delivery failed to topic {Topic}: {Reason}", topic, ex.Error.Reason);
            return default!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while producing message to topic {Topic}", topic);
            return default!;
        }
    }
}
