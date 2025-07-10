using Confluent.Kafka;

namespace Ecommerce.Common.Services.Kafka;

public class KafkaTopicCheckerService : IKafkaTopicCheckerService
{
    private readonly IAdminClient _adminClient;

    public KafkaTopicCheckerService(IAdminClient adminClient)
    {
        _adminClient = adminClient;
    }

    public bool TopicExists(string topic)
    {
        var metadata = _adminClient.GetMetadata(topic, TimeSpan.FromSeconds(5));
        return metadata.Topics.Any(t => t.Topic == topic && t.Error.Code == ErrorCode.NoError);
    }
}

