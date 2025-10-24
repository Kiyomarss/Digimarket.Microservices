using MassTransit;
using Ordering.Core.Consumers;
using Ordering.Core.ServiceContracts;

namespace Ordering.Core.Services;

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