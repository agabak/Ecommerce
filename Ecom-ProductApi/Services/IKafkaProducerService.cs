namespace Ecom_ProductApi.Services;

public interface IKafkaProducerService
{
    Task ProduceAsync(string topic, string message, CancellationToken cancellationToken = default);
}

