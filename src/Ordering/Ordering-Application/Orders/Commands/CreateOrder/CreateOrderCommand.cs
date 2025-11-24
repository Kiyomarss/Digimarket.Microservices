
using BuildingBlocks.CQRS;

namespace Ordering.Application.Orders.Commands.CreateOrder;

public class CreateOrderCommand : ICommand<Guid>
{
    public string Customer { get; set; } = string.Empty;
    public List<OrderItemDto> Items { get; set; } = new();

    public class OrderItemDto
    {
        public string ProductId { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}