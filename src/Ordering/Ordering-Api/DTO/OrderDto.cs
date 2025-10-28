namespace Ordering.Api.DTO
{
    public class OrderDto
    {
        public string Customer { get; set; } = null!;

        public List<OrderItemDto> Items { get; set; } = new();
    }

    public class OrderItemDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public int Quantity { get; set; }
        public int Price { get; set; }
    }
}