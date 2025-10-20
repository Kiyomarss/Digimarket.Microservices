using MassTransit;
using Microsoft.Extensions.Logging;
using Ordering.Components.DTO;

namespace Ordering.Components.Consumers;

public class OrderReadyToProcessConsumer :
    IConsumer<ProcessingStarted>
{
    readonly ILogger<OrderReadyToProcessConsumer> _logger;

    public OrderReadyToProcessConsumer(ILogger<OrderReadyToProcessConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<ProcessingStarted> context)
    {
        _logger.LogInformation("Member {MemberId} registered for event {EventId} on {RegistrationDate}");

        return Task.CompletedTask;
    }
}