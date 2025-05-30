namespace Catalog.Components.Services;

public interface ICatalogValidationService
{
    Task ValidateCatalogs(Guid registrationId);
}