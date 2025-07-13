using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Common.Services.Kafka;

public class ConsumerService(
    IConsumer<string, string> _consumer,
    ILogger<IConsumerService> _logger,
    IConfiguration configuration
) : IConsumerService
{
    public async Task ProcessAsync(string topic, Func<string, Task> messageHandler, CancellationToken cancellationToken = default)
    {
        var bootstrapServers = configuration?.GetSection("ConsumerSettings:BootstrapServers")?.Value;
        if (string.IsNullOrWhiteSpace(bootstrapServers))
        {
            _logger.LogError("Kafka bootstrap servers are not configured.");
            return;
        }

        // Single metadata call to check both: is Kafka up, and is topic available.
        if (!TryGetTopicMetadata(bootstrapServers, topic, out var topicExists))
        {
            _logger.LogError("Kafka is not running or not reachable at {BootstrapServers}.", bootstrapServers);
            return;
        }

        if (!topicExists)
        {
            _logger.LogError("Kafka topic '{Topic}' does not exist.", topic);
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
                    var result = await Task.Run(() =>  _consumer.Consume(cancellationToken));

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
                            // Optionally handle poison message here
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

    /// <summary>
    /// Checks if Kafka is running and returns whether the topic exists.
    /// </summary>
    /// <param name="bootstrapServers">Kafka bootstrap servers.</param>
    /// <param name="topic">Topic to check.</param>
    /// <param name="topicExists">True if topic exists, false otherwise.</param>
    /// <returns>True if Kafka is up (regardless of topic existence), false otherwise.</returns>
    private bool TryGetTopicMetadata(string bootstrapServers, string topic, out bool topicExists)
    {
        topicExists = false;
        try
        {
            using var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = bootstrapServers }).Build();
            var metadata = adminClient.GetMetadata(topic, TimeSpan.FromSeconds(5));
            topicExists = metadata.Topics.Any(t => t.Topic == topic && t.Error.Code == ErrorCode.NoError);
            return true; // Kafka is reachable
        }
        catch (KafkaException kex)
        {
            _logger.LogError(kex, "Error reaching Kafka at {BootstrapServers}: {Error}", bootstrapServers, kex.Message);
            return false; // Kafka not reachable
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while checking Kafka metadata.");
            return false;
        }
    }
}
