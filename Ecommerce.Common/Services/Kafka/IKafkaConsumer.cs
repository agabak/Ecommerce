using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Common.Services.Kafka;

public interface IKafkaConsumer: IDisposable
{
    void Subscribe(string topic);
    ConsumeResult<string, string> Consume(TimeSpan timeout);
    void Close();
}
