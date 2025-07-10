using Confluent.Kafka;
using Microsoft.Extensions.Configuration;

namespace Ecommerce.Common.Services.Kafka;

public static class KafkaExtensions
{
    /// <summary>
    /// Checks if the specified Kafka topic exists.
    /// </summary>
    /// <param name="configuration">The application configuration containing Kafka settings.</param>
    /// <param name="topic">The Kafka topic name.</param>
    /// <param name="bootstrapServersSection">Config section key for bootstrap servers (default: "ConsumerSettings:BootstrapServers").</param>
    /// <returns>True if topic exists, false otherwise.</returns>
    public static bool KafkaTopicExists(this IConfiguration configuration, string topic, string bootstrapServersSection = "ConsumerSettings:BootstrapServers")
    {
        var bootstrapServers = configuration.GetSection(bootstrapServersSection).Value;
        if (string.IsNullOrWhiteSpace(bootstrapServers))
            throw new InvalidOperationException("Kafka bootstrap servers not configured.");

        var config = new AdminClientConfig { BootstrapServers = bootstrapServers };

        using var adminClient = new AdminClientBuilder(config).Build();
        var metadata = adminClient.GetMetadata(topic, TimeSpan.FromSeconds(5));
        return metadata.Topics.Any(t => t.Topic == topic && t.Error.Code == ErrorCode.NoError);
    }
}