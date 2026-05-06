namespace Ordering.Worker.StateMachines.Contracts.Dtos;

public record OrderItemDto(
    Guid ProductId,
    int Quantity);