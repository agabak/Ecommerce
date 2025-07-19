using ECom.Infrastructure.DataAccess.Order.Services;
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
                var result = await HandlePaymentCompletedMessageAsync(message, stoppingToken);
                if (!result)
                {
                    _logger.LogWarning("Failed to process payment completed message: {Message}", message);
                    // Optionally: Save to a dead-letter queue or log for later retry
                }
            },
            cancellationToken: stoppingToken
        );
    }

    private async Task<bool> HandlePaymentCompletedMessageAsync(string message, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(message?.Trim(), out var orderId))
        {
            _logger.LogWarning("Invalid payment completed message format: {Message}", message);
            return false;
        }

        try
        {
            await ProcessStatusUpdateAsync(orderId, cancellationToken);
            var produced = await ProcessOrderNotificationAsync(message, orderId, cancellationToken);

            _logger.LogInformation("Processed payment completed for OrderId: {OrderId}", orderId);
            return produced;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment completed message: {Message}", message);
            return false;
        }
    }

    private async Task<bool> ProcessOrderNotificationAsync(string message, Guid orderId, CancellationToken cancellationToken)
    {
        var deliveryResult = await _producerService.ProduceAsync(
            topic: TopicOrder_Notification,
            key: orderId.ToString(),
            message: message,
            cancellationToken: cancellationToken
        );

        if (deliveryResult.Status != Confluent.Kafka.PersistenceStatus.Persisted)
        {
            _logger.LogError("Failed to produce message to topic {Topic} for OrderId: {OrderId}. Status: {Status}",
                TopicOrder_Notification, orderId, deliveryResult.Status);
            return false;
        }

        _logger.LogInformation("Produced message to topic {Topic} for OrderId: {OrderId}", TopicOrder_Notification, orderId);
        return true;
    }

    public async Task ProcessStatusUpdateAsync(Guid orderId, CancellationToken cancellationToken)
    {
        using var scope = _provider.CreateScope();
        var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();
        var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

        try
        {
            await paymentService.UpdatePaymentStatus(orderId, "Processed", cancellationToken);
            await orderService.UpdateOrderStatusAsync(orderId, "Paid", cancellationToken);

            _logger.LogInformation("Payment and order status updated for OrderId: {OrderId}", orderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating payment/order status for OrderId: {OrderId}", orderId);
            throw; // let outer handler catch and log as failed
        }
    }
}



