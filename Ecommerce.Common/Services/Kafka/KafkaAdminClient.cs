using Confluent.Kafka;

namespace Ecommerce.Common.Services.Kafka;

public class KafkaAdminClient : IKafkaAdminClient
{
    public bool TryGetTopicMetadata(string bootstrapServers, string topic, out bool topicExists)
    {
        topicExists = false;
        try
        {
            using var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = bootstrapServers }).Build();
            var metadata = adminClient.GetMetadata(topic, TimeSpan.FromSeconds(5));
            topicExists = metadata.Topics.Any(t => t.Topic == topic && t.Error.Code == ErrorCode.NoError);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

