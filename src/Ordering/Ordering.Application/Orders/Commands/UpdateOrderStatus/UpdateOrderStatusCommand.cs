
using BuildingBlocks.CQRS;

namespace Ordering.Application.Orders.Commands.CreateOrder;

public class UpdateOrderStatusCommand : ICommand
{
    public Guid Id { get; set; }
    public string State { get; set; }
}