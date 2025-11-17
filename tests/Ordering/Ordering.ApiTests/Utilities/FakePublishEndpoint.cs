using MassTransit;

namespace Ordering.ApiTests.Utilities;

public class FakePublishEndpoint : IPublishEndpoint, ISendEndpointProvider
{
    // return a fake send endpoint for GetPublishSendEndpoint<T>()
    public Task<ISendEndpoint> GetPublishSendEndpoint<T>() where T : class
        => Task.FromResult<ISendEndpoint>(new FakeSendEndpoint());

    // ISendEndpointProvider implementation
    public Task<ISendEndpoint> GetSendEndpoint(Uri address)
        => Task.FromResult<ISendEndpoint>(new FakeSendEndpoint());

    // Publish overloads (no-op)
    public Task Publish<T>(T message, CancellationToken cancellationToken = default) where T : class
        => Task.CompletedTask;

    public Task Publish<T>(T message, IPipe<PublishContext<T>> publishPipe, CancellationToken cancellationToken = default) where T : class
        => Task.CompletedTask;

    public Task Publish<T>(T message, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default) where T : class
        => Task.CompletedTask;

    public Task Publish<T>(object values, CancellationToken cancellationToken = default) where T : class
        => Task.CompletedTask;

    public Task Publish<T>(object values, IPipe<PublishContext<T>> publishPipe, CancellationToken cancellationToken = default) where T : class
        => Task.CompletedTask;

    public Task Publish<T>(object values, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default) where T : class
        => Task.CompletedTask;

    public Task Publish(object message, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task Publish(object message, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task Publish(object message, Type messageType, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task Publish(object message, Type messageType, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    // observers/connects (no-op)
    public ConnectHandle ConnectPublishObserver(IPublishObserver observer)
        => new NoopConnectHandle();

    public ConnectHandle ConnectSendObserver(ISendObserver observer)
        => new NoopConnectHandle();
}