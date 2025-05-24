using MassTransit;
using Microsoft.Extensions.Logging;
using Ordering.Components.Contracts;

namespace Ordering.Components.Consumers;

public class SendOrderEmailConsumer :
    IConsumer<SendRegistrationEmail>
{
    readonly ILogger<SendOrderEmailConsumer> _logger;

    public SendOrderEmailConsumer(ILogger<SendOrderEmailConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<SendRegistrationEmail> context)
    {
        _logger.LogInformation("Notifying Member {MemberId} that they registered for event {EventId} on {RegistrationDate}", context.Message.Customer,
            context.Message.EventId, context.Message.RegistrationDate);

        return Task.CompletedTask;
    }
}