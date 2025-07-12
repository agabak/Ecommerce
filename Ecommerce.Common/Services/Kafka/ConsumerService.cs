using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Common.Services.Kafka;

public class ConsumerService(
    IConsumer<Null, string> _consumer,
    ILogger<IConsumerService> _logger,
    IConfiguration configuration
) : IConsumerService
{
    public async Task ProcessAsync(string topic, Func<string, Task> messageHandler, CancellationToken cancellationToken = default)
    {
        // Check if the topic exists
        var config = new AdminClientConfig { BootstrapServers = configuration.GetSection("ConsumerSettings:BootstrapServers").Value };
        try
        {
            using var adminClient = new AdminClientBuilder(config).Build();
            var metadata = adminClient.GetMetadata(topic, TimeSpan.FromSeconds(5));

            bool topicExists = metadata.Topics.Any(t => t.Topic == topic && t.Error.Code == ErrorCode.NoError);

            if (!topicExists)
            {
                _logger.LogError("Kafka topic '{Topic}' does not exist. Aborting consumer start.", topic);
                return;
            }

            var topicMeta = metadata.Topics.First(t => t.Topic == topic);
            _logger.LogInformation("Topic '{Topic}' found with {Partitions} partitions.", topic, topicMeta.Partitions.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Kafka topic metadata.");
            return;
        }

        _consumer.Subscribe(topic);
        _logger.LogInformation("Subscribed to Kafka topic: {Topic}", topic);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var result = _consumer.Consume(cancellationToken); // Now honors cancellation
                    if (result != null && result.Message?.Value != null)
                    {
                        _logger.LogInformation("Received message: {Message} | Partition: {Partition} | Offset: {Offset}",
                            result.Message.Value, result.Partition, result.Offset);

                        try
                        {
                            await messageHandler(result.Message.Value);
                        }
                        catch (Exception handlerEx)
                        {
                            _logger.LogError(handlerEx, "Error processing Kafka message: {Message}", result.Message.Value);
                            // TODO: Dead-letter or handle poison message if necessary
                        }
                    }
                    else
                    {
                        _logger.LogDebug("No message received in this interval.");
                    }
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Kafka consume error: {Reason}", ex.Error.Reason);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    // Graceful shutdown, just break
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error in Kafka consumer.");
                }
            }
        }
        finally
        {
            try
            {
                _consumer.Close();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error closing Kafka consumer.");
            }
            _logger.LogInformation("Kafka consumer closed for topic: {Topic}", topic);
        }
    }
}
