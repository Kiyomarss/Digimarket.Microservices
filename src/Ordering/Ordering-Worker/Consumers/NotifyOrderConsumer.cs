using MassTransit;
using Ordering.Worker.StateMachines.Events;
using Shared.IntegrationEvents.Ordering;

namespace Ordering.Worker.Consumers;

public class NotifyOrderConsumer :
    IConsumer<OrderInitiated>
{
    readonly ILogger<NotifyOrderConsumer> _logger;

    public NotifyOrderConsumer(ILogger<NotifyOrderConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<OrderInitiated> context)
    {
        return Task.CompletedTask;
    }
}