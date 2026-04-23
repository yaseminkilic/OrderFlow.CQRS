using System.Collections.Concurrent;
using OrderFlow.CQRS.Application.Interfaces;

namespace OrderFlow.CQRS.API.Tests.Fixtures;

public sealed class FakeMessagePublisher : IMessagePublisher
{
    public ConcurrentBag<(object Message, string Queue)> Published { get; } = new();

    public Task PublishAsync<T>(T message, string queueName, CancellationToken cancellationToken = default)
        where T : class
    {
        Published.Add((message, queueName));
        return Task.CompletedTask;
    }
}
