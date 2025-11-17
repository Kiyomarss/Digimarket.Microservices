using MassTransit;

namespace Ordering.ApiTests.Utilities;

public class FakeSendEndpoint : ISendEndpoint
{
    public Uri Address => new Uri("loopback://localhost");

    public Task Send<T>(T message, CancellationToken cancellationToken) where T : class
        => Task.CompletedTask;

    public Task Send<T>(T message, IPipe<SendContext<T>> pipe, CancellationToken cancellationToken) where T : class
        => Task.CompletedTask;

    public Task Send<T>(T message, IPipe<SendContext> pipe, CancellationToken cancellationToken) where T : class
        => Task.CompletedTask;

    public Task Send(object message, CancellationToken cancellationToken)
        => Task.CompletedTask;

    public Task Send(object message, Type messageType, CancellationToken cancellationToken)
        => Task.CompletedTask;

    public Task Send(object message, IPipe<SendContext> pipe, CancellationToken cancellationToken)
        => Task.CompletedTask;

    public Task Send(object message, Type messageType, IPipe<SendContext> pipe, CancellationToken cancellationToken)
        => Task.CompletedTask;

    public Task Send<T>(object values, CancellationToken cancellationToken) where T : class
        => Task.CompletedTask;

    public Task Send<T>(object values, IPipe<SendContext<T>> pipe, CancellationToken cancellationToken) where T : class
        => Task.CompletedTask;

    public Task Send<T>(object values, IPipe<SendContext> pipe, CancellationToken cancellationToken) where T : class
        => Task.CompletedTask;

    public ConnectHandle ConnectSendObserver(ISendObserver observer)
        => new NoopConnectHandle();
}