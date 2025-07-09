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
        using var adminClient = new AdminClientBuilder(config).Build();
        var metadata = adminClient.GetMetadata(topic, TimeSpan.FromSeconds(5));

        bool topicExists = metadata.Topics.Any(t => t.Topic == topic && t.Error.Code == ErrorCode.NoError);

        if (!topicExists)
        {
            _logger.LogError("Kafka topic '{Topic}' does not exist. Aborting consumer start.", topic);
            return;
        }

        // Log topic metadata for diagnostics
        var topicMeta = metadata.Topics.First(t => t.Topic == topic);
        _logger.LogInformation("Topic '{Topic}' found with {Partitions} partitions.", topic, topicMeta.Partitions.Count);

        // Log partition assignment events
        ////_consumer.PositionTopicPartitionOffset += (_, partitions) =>
        ////{
        ////    _logger.LogInformation("Partitions assigned: {Partitions}", string.Join(",", partitions.Select(p => p.Partition.Value)));
        ////};
        ////_consumer.OnPartitionsRevoked += (_, partitions) =>
        ////{
        ////    _logger.LogInformation("Partitions revoked: {Partitions}", string.Join(",", partitions.Select(p => p.Partition.Value)));
        ////};

        _consumer.Subscribe(topic);
        _logger.LogInformation("Subscribed to Kafka topic: {Topic}", topic);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Use a timeout to avoid indefinite blocking
                    var result = _consumer.Consume(TimeSpan.FromSeconds(1));
                    if (result != null && result.Message?.Value != null)
                    {
                        _logger.LogInformation("Received message: {Message}", result.Message.Value);
                        await messageHandler(result.Message.Value);
                    }
                    else
                    {
                        // Optional: log heartbeat or do other work
                        //_logger.LogDebug("No message received in this interval.");
                    }
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Kafka consume error: {Reason}", ex.Error.Reason);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error in Kafka consumer.");
                }
            }
        }
        finally
        {
            _consumer.Close();
            _logger.LogInformation("Kafka consumer closed for topic: {Topic}", topic);
        }
    }
}
