using MassTransit;
using Microsoft.Extensions.Logging;
using Ordering.Components.Contracts;

namespace Ordering.Components.Consumers;

public class NotifyOrderConsumer :
    IConsumer<OrderSubmitted>
{
    readonly ILogger<NotifyOrderConsumer> _logger;

    public NotifyOrderConsumer(ILogger<NotifyOrderConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<OrderSubmitted> context)
    {
        _logger.LogInformation("Member {MemberId} registered for event {EventId} on {RegistrationDate}", context.Message.Customer,
            context.Message.RegistrationDate);

        return Task.CompletedTask;
    }
}