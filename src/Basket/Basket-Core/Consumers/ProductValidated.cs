namespace Basket.Core.Consumers;

public record BasketValidated
{
    public Guid RegistrationId { get; init; }
}