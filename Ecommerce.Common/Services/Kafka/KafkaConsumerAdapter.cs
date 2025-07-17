using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Common.Services.Kafka;

public class KafkaConsumerAdapter : IKafkaConsumer
{
    private readonly IConsumer<string, string> _consumer;

    public KafkaConsumerAdapter(IConsumer<string, string> consumer)
    {
        _consumer = consumer;
    }

    public void Subscribe(string topic) => _consumer.Subscribe(topic);
    public ConsumeResult<string, string> Consume(TimeSpan timeout) => _consumer.Consume(timeout);
    public void Close() => _consumer.Close();
    public void Dispose() => _consumer.Dispose();
}

