using Ordering_Domain.Domain.Entities;

namespace Ordering.UnitTests;

public class OrderItemTests
{
    [Fact]
    public void OrderItem_Should_Set_Properties()
    {
        var productId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var item = new OrderItem(productId, "Test", 3, 50);

        Assert.Equal(productId, item.ProductId);
        Assert.Equal("Test", item.ProductName);
        Assert.Equal(3, item.Quantity);
        Assert.Equal(50, item.Price);
    }
}