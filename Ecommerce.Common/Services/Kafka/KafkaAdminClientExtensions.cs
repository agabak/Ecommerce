using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Common.Services.Kafka;

public static class KafkaAdminClientExtensions
{
    public static bool TopicExists(this IAdminClient adminClient, string topic)
    {
        var metadata = adminClient.GetMetadata(topic, TimeSpan.FromSeconds(5));
        return metadata.Topics.Any(t => t.Topic == topic && t.Error.Code == ErrorCode.NoError);
    }
}
