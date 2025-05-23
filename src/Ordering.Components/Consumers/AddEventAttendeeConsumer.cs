using MassTransit;
using Microsoft.Extensions.Logging;
using Ordering.Components.Contracts;

namespace Ordering.Components.Consumers;

public class AddEventAttendeeConsumer :
    IConsumer<AddEventAttendee>
{
    readonly ILogger<AddEventAttendeeConsumer> _logger;

    public AddEventAttendeeConsumer(ILogger<AddEventAttendeeConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<AddEventAttendee> context)
    {
        _logger.LogInformation("Adding Member {MemberId} as an attendee for event {EventId}", context.Message.Customer);

        return Task.CompletedTask;
    }
}