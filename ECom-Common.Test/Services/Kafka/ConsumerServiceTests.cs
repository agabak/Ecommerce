using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using Ecommerce.Common.Services.Kafka;

namespace Ecommerce.Common.Tests.Services.Kafka
{
    [TestFixture]
    public class ConsumerServiceTests
    {
        private IKafkaConsumer _mockConsumer;
        private ILogger<IConsumerService> _mockLogger;
        private IConfiguration _mockConfig;
        private IKafkaAdminClient _mockAdminClient;
        private ConsumerService _service;

        private IConfigurationSection SetupConfigSection(string key, string value)
        {
            var section = Substitute.For<IConfigurationSection>();
            section.Value.Returns(value);
            _mockConfig.GetSection(key).Returns(section);
            return section;
        }

        [SetUp]
        public void SetUp()
        {
            _mockConsumer = Substitute.For<IKafkaConsumer>();
            _mockLogger = Substitute.For<ILogger<IConsumerService>>();
            _mockConfig = Substitute.For<IConfiguration>();
            _mockAdminClient = Substitute.For<IKafkaAdminClient>();
            _service = new ConsumerService(_mockConsumer, _mockLogger, _mockConfig, _mockAdminClient);
        }

        [TearDown]
        public void TearDown()
        {
            _mockConsumer?.Dispose();
        }

        [Test]
        public async Task ProcessAsync_LogsAndReturns_WhenBootstrapServersMissing()
        {
            SetupConfigSection("ConsumerSettings:BootstrapServers", null);

            await _service.ProcessAsync("mytopic", msg => Task.CompletedTask);

            _mockLogger.Received().Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString().Contains("Kafka bootstrap servers are not configured.")),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>());
            _mockConsumer.DidNotReceive().Subscribe(Arg.Any<string>());
        }

        [Test]
        public async Task ProcessAsync_LogsAndReturns_WhenKafkaNotReachable()
        {
            SetupConfigSection("ConsumerSettings:BootstrapServers", "my-server:9092");
            _mockAdminClient
                .TryGetTopicMetadata("my-server:9092", "mytopic", out Arg.Any<bool>())
                .Returns(x => { x[2] = false; return false; });

            await _service.ProcessAsync("mytopic", msg => Task.CompletedTask);

            _mockLogger.Received().Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString().Contains("Kafka is not running or not reachable")),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>());
            _mockConsumer.DidNotReceive().Subscribe(Arg.Any<string>());
        }

        [Test]
        public async Task ProcessAsync_LogsAndReturns_WhenTopicNotFound()
        {
            SetupConfigSection("ConsumerSettings:BootstrapServers", "my-server:9092");
            _mockAdminClient
                .TryGetTopicMetadata("my-server:9092", "mytopic", out Arg.Any<bool>())
                .Returns(x => { x[2] = false; return true; });

            await _service.ProcessAsync("mytopic", msg => Task.CompletedTask);

            _mockLogger.Received().Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString().Contains("Kafka topic")),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>());
            _mockConsumer.DidNotReceive().Subscribe(Arg.Any<string>());
        }

        [Test]
        public async Task ProcessAsync_ConsumesAndCallsHandler_WhenMessageReceived()
        {
            SetupConfigSection("ConsumerSettings:BootstrapServers", "my-server:9092");
            _mockAdminClient
                .TryGetTopicMetadata("my-server:9092", "mytopic", out Arg.Any<bool>())
                .Returns(x => { x[2] = true; return true; });

            var message = new Message<string, string> { Value = "test-message" };
            var consumeResult = new ConsumeResult<string, string>
            {
                Message = message,
                Topic = "mytopic",
                Partition = new Partition(0),
                Offset = new Offset(42)
            };

            bool handlerCalled = false;
            Func<string, Task> handler = async msg =>
            {
                handlerCalled = (msg == "test-message");
                await Task.CompletedTask;
            };

            _mockConsumer.Consume(Arg.Any<TimeSpan>()).Returns(consumeResult, (ConsumeResult<string, string>)null);

            // CancellationToken triggers after one loop to avoid infinite loop
            var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));
            await _service.ProcessAsync("mytopic", handler, cts.Token);

            Assert.IsTrue(handlerCalled);
            _mockConsumer.Received().Subscribe("mytopic");
        }

        [Test]
        public async Task ProcessAsync_HandlesConsumeException_AndContinuesLoop()
        {
            SetupConfigSection("ConsumerSettings:BootstrapServers", "my-server:9092");
            _mockAdminClient
                .TryGetTopicMetadata("my-server:9092", "mytopic", out Arg.Any<bool>())
                .Returns(x => { x[2] = true; return true; });

            int callCount = 0;
            _mockConsumer.Consume(Arg.Any<TimeSpan>()).Returns(ci =>
            {
                if (callCount++ == 0)
                    throw new ConsumeException(
                        new ConsumeResult<byte[], byte[]>(), // CORRECT type for exception!
                        new Error(ErrorCode.Unknown, "fail"));
                return null;
            });

            var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));
            await _service.ProcessAsync("mytopic", msg => Task.CompletedTask, cts.Token);

            _mockLogger.Received().Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<ConsumeException>(),
                Arg.Any<Func<object, Exception, string>>());
        }
    }
}

