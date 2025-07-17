using Confluent.Kafka;
using Ecommerce.Common.Services.Kafka;
using Ecommerce.Common.Settings.Extension;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECom_Common.Test.Extensions
{
    [TestFixture]
    public class ServiceCollectionExtensionsTest
    {
        private static readonly Dictionary<string, string> _kafkaConfigDict = new()
        {
            {"ConsumerSettings:BootstrapServers", "localhost:9092"},
            {"ConsumerSettings:GroupId", "test-group"},
            {"ProducerSettings:BootstrapServers", "localhost:9092"},
            {"ProducerSettings:Topic", "test-topic"}
        };

        private ServiceCollection CreateServiceCollection(IConfiguration config)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton(config);
            return services;
        }

        private IConfiguration BuildKafkaConfig() =>
            new ConfigurationBuilder()
                .AddInMemoryCollection(_kafkaConfigDict)
                .Build();

        [Test(Description = "Ensures AddKafkaConsumerProducer registers all required Kafka consumer and producer services and dependencies in the DI container.")]
        public void AddKafkaConsumerProducer_RegistersServices()
        {
            // Arrange
            var config = BuildKafkaConfig();
            var services = CreateServiceCollection(config);

            // Act
            services.AddKafkaConsumerProducer(config);
            using var provider = services.BuildServiceProvider();

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(provider.GetService<IKafkaConsumer>(), Is.Not.Null);
                Assert.That(provider.GetService<IKafkaAdminClient>(), Is.Not.Null);
                Assert.That(provider.GetService<IConsumerService>(), Is.Not.Null);
                Assert.That(provider.GetService<IProducerService>(), Is.Not.Null);
                Assert.That(provider.GetService<IProducer<string, string>>(), Is.Not.Null);
                Assert.That(provider.GetService<IConsumer<string, string>>(), Is.Not.Null);
            });
        }

        [Test(Description = "Verifies that calling AddKafkaConsumerProducer with an empty configuration does not result in an exception.")]
        public void AddKafkaConsumerProducer_WithMissingConfig_DoesNotThrow()
        {
            var config = new ConfigurationBuilder().Build();
            var services = CreateServiceCollection(config);

            Assert.DoesNotThrow(() => services.AddKafkaConsumerProducer(config));
        }

        [Test(Description = "Check that key services are registered as singletons (or as expected).")]
        public void AddKafkaConsumerProducer_RegistersSingletonLifetimes()
        {
            var config = BuildKafkaConfig();
            var services = CreateServiceCollection(config);
            services.AddKafkaConsumerProducer(config);

            var descriptorProducer = services.FirstOrDefault(d => d.ServiceType == typeof(IProducerService));
            var descriptorConsumer = services.FirstOrDefault(d => d.ServiceType == typeof(IConsumerService));
            var descriptorKafkaAdmin = services.FirstOrDefault(d => d.ServiceType == typeof(IKafkaAdminClient));
            var descriptorKafkaConsumer = services.FirstOrDefault(d => d.ServiceType == typeof(IKafkaConsumer));

            Assert.Multiple(() =>
            {
                Assert.That(descriptorProducer?.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
                Assert.That(descriptorConsumer?.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
                Assert.That(descriptorKafkaAdmin?.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
                Assert.That(descriptorKafkaConsumer?.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
            });
        }

        [Test(Description = "Confirm that multiple resolutions return the same instance for singletons.")]
        public void AddKafkaConsumerProducer_ResolvesSingletons_AsSameInstance()
        {
            var config = BuildKafkaConfig();
            var services = CreateServiceCollection(config);
            services.AddKafkaConsumerProducer(config);

            using var provider = services.BuildServiceProvider();

            var instance1 = provider.GetService<IProducerService>();
            var instance2 = provider.GetService<IProducerService>();
            var consumerInstance1 = provider.GetService<IConsumerService>();
            var consumerInstance2 = provider.GetService<IConsumerService>();
            var kafkaAdminInstance1 = provider.GetService<IKafkaAdminClient>();
            var kafkaAdminInstance2 = provider.GetService<IKafkaAdminClient>();
            var kafkaConsumerInstance1 = provider.GetService<IKafkaConsumer>();
            var kafkaConsumerInstance2 = provider.GetService<IKafkaConsumer>();

            Assert.Multiple(() =>
            {
                Assert.That(instance1, Is.SameAs(instance2));
                Assert.That(consumerInstance1, Is.SameAs(consumerInstance2));
                Assert.That(kafkaAdminInstance1, Is.SameAs(kafkaAdminInstance2));
                Assert.That(kafkaConsumerInstance1, Is.SameAs(kafkaConsumerInstance2));
            });
        }
    }
}
