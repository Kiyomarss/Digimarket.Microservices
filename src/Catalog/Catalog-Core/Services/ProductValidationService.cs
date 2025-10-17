using Catalog.Components.Consumers;
using MassTransit;

namespace Catalog.Components.Services;

public class ProductValidationService :
    IProductValidationService
{
    readonly IPublishEndpoint _publishEndpoint;

    public ProductValidationService(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task ValidateCatalogs(Guid registrationId)
    {
        await _publishEndpoint.Publish(new CatalogValidated { RegistrationId = registrationId });
    }
}