using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Common.Services.Kafka;

public class ConsumerService(IConsumer<Null, string> _consumer, ILogger<IConsumerService> _logger, IConfiguration configuration)
    : IConsumerService
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
            //throw new InvalidOperationException($"Kafka topic '{topic}' does not exist.");
        }

        _consumer.Subscribe(topic);
        _logger.LogInformation("Started Kafka consumer for topic: {Topic}", topic);
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var result = _consumer.Consume(cancellationToken);
                    if (result?.Message?.Value != null)
                    {
                        _logger.LogInformation("Received message: {Message}", result.Message.Value);
                        await messageHandler(result.Message.Value);
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
        }
    }
}

