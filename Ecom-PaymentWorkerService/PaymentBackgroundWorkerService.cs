using Ecom_PaymentWorkerService.Services;
using Ecommerce.Common.Services.Kafka;

namespace Ecom_PaymentWorkerService;

public class PaymentBackgroundWorkerService : BackgroundService
{
    private const string TopicPayment_Completed = "Payment.Completed";
    private const string TopicOrder_Notification = "Order.Notification";
    private readonly IConsumerService _consumer;
    private readonly IServiceProvider _provider; 
    private readonly IProducerService _producerService; 
    private readonly ILogger<PaymentBackgroundWorkerService> _logger;

    public PaymentBackgroundWorkerService(
        IConsumerService consumer,
        IServiceProvider provider,
        IProducerService producerService,    
        ILogger<PaymentBackgroundWorkerService> logger)
    {
        _consumer = consumer;
        _provider = provider;
        _producerService = producerService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PaymentBackgroundWorkerService started. Subscribing to topic: {Topic}", TopicPayment_Completed);

        await _consumer.ProcessAsync(
            topic: TopicPayment_Completed,
            messageHandler: async (message) =>
            {
                await HandlePaymentCompletedMessageAsync(message, stoppingToken);
            },
            cancellationToken: stoppingToken
        );
    }

    private async Task HandlePaymentCompletedMessageAsync(string message, CancellationToken stoppingToken)
    {
        using var scope = _provider.CreateScope();
        var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();
 
        try
        {
            if (Guid.TryParse(message?.Trim(), out var orderId))
            {
                await paymentService.ProcessStatusAsync(orderId, stoppingToken);
              var deliveryResult =   await _producerService.ProduceAsync(
                    topic: TopicOrder_Notification,
                    key: orderId.ToString(),
                    message: message,
                    cancellationToken: stoppingToken
                );

                if (deliveryResult.Status != Confluent.Kafka.PersistenceStatus.Persisted)
                {
                    _logger.LogError("Failed to produce message to topic {Topic} for OrderId: {OrderId}. Status: {Status}",
                        TopicOrder_Notification, orderId, deliveryResult.Status);
                }
                else
                {
                    _logger.LogInformation("Produced message to topic {Topic} for OrderId: {OrderId}", 
                        TopicOrder_Notification, orderId);
                }

                _logger.LogInformation("Processed payment completed for OrderId: {OrderId}", orderId);
            }
            else
            {
                _logger.LogWarning("Invalid payment completed message format: {Message}", message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment completed message: {Message}", message);
        }
    }
}


