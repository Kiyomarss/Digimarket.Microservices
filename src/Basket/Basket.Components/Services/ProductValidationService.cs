using Basket.Components.Consumers;
using MassTransit;

namespace Basket.Components.Services;

public class ProductValidationService :
    IProductValidationService
{
    readonly IPublishEndpoint _publishEndpoint;

    public ProductValidationService(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task ValidateBaskets(Guid registrationId)
    {
        await _publishEndpoint.Publish(new BasketValidated { RegistrationId = registrationId });
    }
}