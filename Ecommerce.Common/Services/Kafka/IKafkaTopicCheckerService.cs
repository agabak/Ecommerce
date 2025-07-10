namespace Ecommerce.Common.Services.Kafka
{
    public interface IKafkaTopicCheckerService
    {
        bool TopicExists(string topic);
    }
}