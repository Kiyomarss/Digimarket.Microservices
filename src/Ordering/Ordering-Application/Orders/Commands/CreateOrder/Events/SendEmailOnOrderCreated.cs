using MediatR;
using Microsoft.Extensions.Logging;
using Ordering_Domain.Domain.Events;

namespace Ordering.Application.Orders.Commands.CreateOrder.Events;

public class SendEmailOnOrderCreated : INotificationHandler<OrderCreatedDomainEvent>
{
    private readonly ILogger<SendEmailOnOrderCreated> _logger;

    public SendEmailOnOrderCreated(ILogger<SendEmailOnOrderCreated> logger)
    {
        _logger = logger;
    }

    public Task Handle(OrderCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Order created! Sending confirmation email to customer: {Customer} for OrderId: {OrderId}",
                               notification.Customer);

        return Task.CompletedTask;
    }
}