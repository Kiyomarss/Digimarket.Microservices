namespace Catalog.Core.Consumers;

public record CatalogValidated
{
    public Guid RegistrationId { get; init; }
}