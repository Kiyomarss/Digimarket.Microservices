namespace Catalog.Worker.Consumers;

public record CatalogValidated
{
    public Guid RegistrationId { get; init; }
}