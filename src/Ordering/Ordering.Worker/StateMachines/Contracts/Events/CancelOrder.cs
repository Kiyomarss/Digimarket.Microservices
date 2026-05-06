using Ordering.Worker.StateMachines.Contracts.Dtos;

namespace Ordering.Worker.StateMachines.Contracts.Events;

public record CancelOrder(Guid Id, IEnumerable<OrderItemDto> Items);
