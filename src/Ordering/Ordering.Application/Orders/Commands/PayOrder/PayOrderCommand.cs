
using BuildingBlocks.CQRS;

namespace Ordering.Application.Orders.Commands.PayOrder;

public class PayOrderCommand : ICommand
{
    public Guid Id { get; set; }
}