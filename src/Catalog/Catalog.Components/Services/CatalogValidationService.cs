using Catalog.Components.Consumers;
using MassTransit;

namespace Catalog.Components.Services;

public class CatalogValidationService :
    ICatalogValidationService
{
    readonly IPublishEndpoint _publishEndpoint;

    public CatalogValidationService(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task ValidateCatalogs(Guid registrationId)
    {
        await _publishEndpoint.Publish(new CatalogValidated { RegistrationId = registrationId });
    }
}