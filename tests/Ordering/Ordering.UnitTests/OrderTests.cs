using Ordering_Domain.Domain.Entities;

namespace Ordering.UnitTests;

public class OrderTests
{
    [Fact]
    public void TotalPrice_Should_Calculate_Correctly()
    {
        var order = new Order
        {
            Items =
            {
                new OrderItem(Guid.NewGuid(), "Item1", 2, 100),
                new OrderItem(Guid.NewGuid(), "Item2", 1, 200)
            }
        };

        Assert.Equal(400, order.TotalPrice);
    }
}