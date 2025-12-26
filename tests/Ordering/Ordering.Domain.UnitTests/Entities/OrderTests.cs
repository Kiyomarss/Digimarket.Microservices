using FluentAssertions;
using Ordering_Domain.Domain.Entities;
using Ordering_Domain.Domain.Enum;

namespace Ordering.Domain.UnitTests.Entities;

public class OrderTests
{
    [Fact]
    public void New_Order_Should_Have_Default_Values()
    {
        // Act
        var order = new Order();

        // Assert
        order.Id.Should().NotBe(Guid.Empty);
        order.State.Should().Be(OrderState.Pending);
        order.Items.Should().BeEmpty();
        order.Customer.Should().BeNull();
        order.TotalPrice.Should().Be(0);
    }

    [Fact]
    public void TotalPrice_Should_Be_Sum_Of_Item_Price_x_Quantity()
    {
        // Arrange
        var order = new Order
        {
            Customer = "Ali",
            Items =
            {
                new OrderItem { Price = 1000, Quantity = 2 }, // 2000
                new OrderItem { Price = 500, Quantity = 3 }   // 1500
            }
        };

        // Act
        var total = order.TotalPrice;

        // Assert
        total.Should().Be(3500);
    }

    [Fact]
    public void Adding_OrderItem_Should_Update_TotalPrice()
    {
        // Arrange
        var order = new Order();
        var item = new OrderItem
        {
            Price = 1000,
            Quantity = 3
        };

        // Act
        order.Items.Add(item);

        // Assert
        order.TotalPrice.Should().Be(3000);
    }
}