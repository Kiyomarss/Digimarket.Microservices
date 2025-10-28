using MassTransit;
using Ordering.Worker.StateMachines.Events;

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
        _logger.LogInformation("Member {MemberId} registered for event {EventId} on {RegistrationDate}", context.Message.Customer,
            context.Message.Date);

        return Task.CompletedTask;
    }
}