using MassTransit;
using Microsoft.Extensions.Logging;
using Ordering.Components.Contracts;

namespace Ordering.Components.Consumers;

public class OrderReadyToProcessConsumer :
    IConsumer<OrderReadyToProcess>
{
    readonly ILogger<OrderReadyToProcessConsumer> _logger;

    public OrderReadyToProcessConsumer(ILogger<OrderReadyToProcessConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<OrderReadyToProcess> context)
    {
        _logger.LogInformation("Member {MemberId} registered for event {EventId} on {RegistrationDate}");

        return Task.CompletedTask;
    }
}