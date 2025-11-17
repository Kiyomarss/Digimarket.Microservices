using MassTransit;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ordering.ApiTests.Utilities;

public class FakeBus : IBus, IBusControl
{
    public Uri Address => new Uri("loopback://localhost");
    public IBusTopology Topology => null!;

    public ConnectHandle ConnectPublishObserver(IPublishObserver observer) => new NoopConnectHandle();
    public ConnectHandle ConnectSendObserver(ISendObserver observer) => new NoopConnectHandle();

    public Task<ISendEndpoint> GetSendEndpoint(Uri address) => Task.FromResult<ISendEndpoint>(new FakeSendEndpoint());
    public Task<ISendEndpoint> GetPublishSendEndpoint<T>() where T : class => Task.FromResult<ISendEndpoint>(new FakeSendEndpoint());

    public Task Publish<T>(T message, CancellationToken cancellationToken = default) where T : class => Task.CompletedTask;
    public Task Publish<T>(T message, IPipe<PublishContext<T>> pipe, CancellationToken cancellationToken = default) where T : class => Task.CompletedTask;
    public Task Publish<T>(T message, IPipe<PublishContext> pipe, CancellationToken cancellationToken = default) where T : class => Task.CompletedTask;
    public Task Publish(object message, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task Publish(object message, IPipe<PublishContext> pipe, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task Publish<T>(object values, CancellationToken cancellationToken = default) where T : class => Task.CompletedTask;
    public Task Publish<T>(object values, IPipe<PublishContext<T>> publishPipe, CancellationToken cancellationToken = default) where T : class => Task.CompletedTask;
    public Task Publish<T>(object values, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default) where T : class => Task.CompletedTask;
    public Task Publish(object message, Type messageType, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task Publish(object message, Type messageType, IPipe<PublishContext> pipe, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task StartAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    Task<BusHandle> IBusControl.StartAsync(CancellationToken cancellationToken)
    {
        // FakeBusHandle می‌تواند فقط null برگرداند
        return Task.FromResult<BusHandle>(null!);
    }

    public Task StopAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    // Fake BusHealthResult بدون نیاز به سازنده خصوصی
    public BusHealthResult CheckHealth()
    {
        // به جای استفاده مستقیم از سازنده داخلی، از Extension یا Fake کلاس استفاده می‌کنیم
        return null;
    }

    public void Probe(ProbeContext context) { }

    public ConnectHandle ConnectConsumePipe<T>(IPipe<ConsumeContext<T>> pipe) where T : class => new NoopConnectHandle();
    public ConnectHandle ConnectConsumePipe<T>(IPipe<ConsumeContext<T>> pipe, ConnectPipeOptions options) where T : class => new NoopConnectHandle();
    public ConnectHandle ConnectRequestPipe<T>(Guid requestId, IPipe<ConsumeContext<T>> pipe) where T : class => new NoopConnectHandle();
    public ConnectHandle ConnectConsumeMessageObserver<T>(IConsumeMessageObserver<T> observer) where T : class => new NoopConnectHandle();
    public ConnectHandle ConnectConsumeObserver(IConsumeObserver observer) => new NoopConnectHandle();
    public ConnectHandle ConnectReceiveObserver(IReceiveObserver observer) => new NoopConnectHandle();
    public ConnectHandle ConnectReceiveEndpointObserver(IReceiveEndpointObserver observer) => new NoopConnectHandle();
    public ConnectHandle ConnectEndpointConfigurationObserver(IEndpointConfigurationObserver observer) => new NoopConnectHandle();
    public HostReceiveEndpointHandle ConnectReceiveEndpoint(IEndpointDefinition definition, IEndpointNameFormatter? endpointNameFormatter = null, Action<IReceiveEndpointConfigurator>? configureEndpoint = null) => null!;
    public HostReceiveEndpointHandle ConnectReceiveEndpoint(string queueName, Action<IReceiveEndpointConfigurator>? configureEndpoint = null) => null!;
}