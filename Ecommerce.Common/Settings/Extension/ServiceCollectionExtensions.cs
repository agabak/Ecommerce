using Confluent.Kafka;
using Ecommerce.Common.Services.Kafka;
using Ecommerce.Common.Settings.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Ecommerce.Common.Settings.Extension;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKafkaConsumerProducer(this IServiceCollection services, IConfiguration configuration)
    {
        // Bind and register ConsumerSettings  
        services.Configure<ConsumerSettings>(configuration.GetSection("ConsumerSettings"));

        // Bind and register ProducerSettings  
        services.Configure<ProducerSettings>(configuration.GetSection("ProducerSettings"));

        // (Optional) Register strongly-typed settings for direct injection  
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<ConsumerSettings>>().Value);
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<ProducerSettings>>().Value);

        // Register Kafka consumer using DI-bound settings  
        services.AddSingleton<IConsumer<string, string>>(sp =>
        {
            var settings = sp.GetRequiredService<ConsumerSettings>();
            return new ConsumerBuilder<string, string>(settings).Build();
        });

        // Register Kafka producer using DI-bound settings  
        services.AddSingleton<IProducer<string, string>>(sp =>
        {
            var settings = sp.GetRequiredService<ProducerSettings>();
            return new ProducerBuilder<string, string>(settings).Build();
        });

        services.AddSingleton<IKafkaAdminClient, KafkaAdminClient>();
        services.AddSingleton<IKafkaConsumer, KafkaConsumerAdapter>();

        services.AddSingleton<IProducerService, ProducerService>();
        services.AddSingleton<IConsumerService, ConsumerService>();

        return services;
    }
}
