using Confluent.Kafka;
using Ecom_ProductApi.Repositories;

namespace Ecom_ProductApi.Services;

public class DailyInventoryJobService : BackgroundService
{
    private readonly ILogger<DailyInventoryJobService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public DailyInventoryJobService(ILogger<DailyInventoryJobService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DailyInventoryJobService is running.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IProductRepository>();

                var productIds = await repository.GetAllProductIdsAsync(stoppingToken);
                // 🔁 Your one-time daily job logic
                await RunDailyInventoryTaskAsync(productIds, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while running daily inventory task.");
            }

            // Wait for 24 hours
            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }

    private async Task RunDailyInventoryTaskAsync(List<Guid> productIds, CancellationToken token)
    {
      
        if (productIds == null || !productIds.Any())
        {
            _logger.LogInformation("No products found for daily inventory task.");
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var producerService = scope.ServiceProvider.GetRequiredService<IKafkaProducerService>();
        var respository = scope.ServiceProvider.GetRequiredService<IProductRepository>();
        foreach ( var productId in productIds)
        {
            await producerService.ProduceAsync("Create-Inventory", productId.ToString(), token);
            await respository.UpsertInventoryAsync(productId, token);
        }
    }
}

