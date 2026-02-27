
using BuildingBlocks.CQRS;

namespace Ordering.Application.Orders.Commands.OrderCancelled;

public class OrderCanceledCommand : ICommand
{
    public Guid Id { get; set; }
}