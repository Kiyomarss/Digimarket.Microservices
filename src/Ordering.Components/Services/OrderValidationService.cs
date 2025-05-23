using MassTransit;
using Ordering.Components.Consumers;

namespace Ordering.Components.Services;

public class OrderValidationService :
    IOrderValidationService
{
    readonly IPublishEndpoint _publishEndpoint;

    public OrderValidationService(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task ValidateOrders(Guid registrationId)
    {
        await _publishEndpoint.Publish(new OrderValidated { RegistrationId = registrationId });
    }
}