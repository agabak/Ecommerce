using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Common.Services.Kafka;

public interface IKafkaAdminClient
{
    /// <summary>
    /// Checks if the Kafka topic exists and returns true if Kafka is reachable.
    /// </summary>
    bool TryGetTopicMetadata(string bootstrapServers, string topic, out bool topicExists);
}
