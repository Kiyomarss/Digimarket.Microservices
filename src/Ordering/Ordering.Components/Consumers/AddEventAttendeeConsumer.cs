using MassTransit;
using Microsoft.Extensions.Logging;
using Ordering.Components.Contracts;

namespace Ordering.Components.Consumers;

public class AddEventAttendeeConsumer :
    IConsumer<PaymentValidated>
{
    readonly ILogger<AddEventAttendeeConsumer> _logger;

    public AddEventAttendeeConsumer(ILogger<AddEventAttendeeConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<PaymentValidated> context)
    {
        _logger.LogInformation("Adding Member {MemberId} as an attendee for event {EventId}", context.Message.Customer);

        return Task.CompletedTask;
    }
}