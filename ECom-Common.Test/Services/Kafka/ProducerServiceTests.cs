using Confluent.Kafka;
using Ecommerce.Common.Services.Kafka;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECom_Common.Test.Services.Kafka;



[TestFixture]
public class ProducerServiceTests
{
    private IProducer<string, string> _mockProducer;
    private ILogger<ProducerService> _mockLogger;
    private ProducerService _service;

    [SetUp]
    public void SetUp()
    {
        _mockProducer = Substitute.For<IProducer<string, string>>();
        _mockLogger = Substitute.For<ILogger<ProducerService>>();
        _service = new ProducerService(_mockProducer, _mockLogger);
    }

    [TearDown]
    public void TearDown()
    {
        _mockProducer.Dispose();
    }

    [Test]
    public async Task ProduceAsync_LogsInfo_WhenMessagePersisted()
    {
        // Arrange
        var deliveryResult = new DeliveryResult<string, string>
        {
            Topic = "test-topic",
            Partition = new Partition(1),
            Offset = new Offset(10),
            Status = PersistenceStatus.Persisted
        };
        _mockProducer
            .ProduceAsync("test-topic", Arg.Any<Message<string, string>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(deliveryResult));

        // Act
        await _service.ProduceAsync("test-topic", "key1", "hello");

        // Assert
        _mockLogger.Received().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(v => v.ToString().Contains("Message produced to topic")),
            null,
            Arg.Any<Func<object, Exception, string>>());
    }

    [Test]
    public async Task ProduceAsync_LogsWarning_WhenMessageNotPersisted()
    {
        var deliveryResult = new DeliveryResult<string, string>
        {
            Topic = "test-topic",
            Partition = new Partition(1),
            Offset = new Offset(10),
            Status = PersistenceStatus.NotPersisted
        };
        _mockProducer
            .ProduceAsync("test-topic", Arg.Any<Message<string, string>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(deliveryResult));

        await _service.ProduceAsync("test-topic", "key1", "hello");

        _mockLogger.Received().Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(v => v.ToString().Contains("not persisted")),
            null,
            Arg.Any<Func<object, Exception, string>>());
    }

    [Test]
    public void ProduceAsync_LogsErrorAndThrows_OnProduceException()
    {
        _mockProducer
            .ProduceAsync("test-topic", Arg.Any<Message<string, string>>(), Arg.Any<CancellationToken>())
            .Returns<Task<DeliveryResult<string, string>>>(_ => throw new ProduceException<string, string>(
                new Error(ErrorCode.Unknown, "fail"),
                new DeliveryResult<string, string>()
            ));

        Assert.ThrowsAsync<ProduceException<string, string>>(async () =>
            await _service.ProduceAsync("test-topic", "key1", "hello"));

        _mockLogger.Received().Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(v => v.ToString().Contains("Kafka delivery failed")),
            Arg.Any<ProduceException<string, string>>(),
            Arg.Any<Func<object, Exception, string>>());
    }

    [Test]
    public void ProduceAsync_LogsErrorAndThrows_OnOtherException()
    {
        _mockProducer
            .ProduceAsync("test-topic", Arg.Any<Message<string, string>>(), Arg.Any<CancellationToken>())
            .Returns<Task<DeliveryResult<string, string>>>(_ => throw new InvalidOperationException("fail"));

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _service.ProduceAsync("test-topic", "key1", "hello"));

        _mockLogger.Received().Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(v => v.ToString().Contains("An error occurred")),
            Arg.Any<InvalidOperationException>(),
            Arg.Any<Func<object, Exception, string>>());
    }
}

