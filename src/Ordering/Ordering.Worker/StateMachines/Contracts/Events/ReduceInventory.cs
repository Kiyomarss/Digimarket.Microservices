using Ordering.Worker.StateMachines.Contracts.Dtos;

namespace Ordering.Worker.StateMachines.Contracts.Events;


public record ReduceInventory(IEnumerable<OrderItemDto> Items);

