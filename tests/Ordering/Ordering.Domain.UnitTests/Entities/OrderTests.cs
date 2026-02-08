using BuildingBlocks.Domain;
using FluentAssertions;
using Ordering_Domain.Domain.Entities;
using Ordering_Domain.Domain.Enum;
using Ordering_Domain.DomainEvents;

namespace Ordering.Domain.UnitTests.Entities;

public class OrderTests
{
    [Fact]
    public void New_Order_Should_Have_Default_Values()
    {
        // Act
        var order =  new OrderBuilder().Build();

        // Assert
        order.Id.Should().NotBe(Guid.Empty);
        order.State.Should().Be(OrderState.Pending);
        order.Items.Should().BeEmpty();
        order.TotalPrice.Should().Be(0);
    }

    [Fact]
    public void TotalPrice_Should_Be_Sum_Of_Item_Price_x_Quantity()
    {
        // Arrange
        var order =  new OrderBuilder().Build();

        order.AddItem(Guid.NewGuid(), $"Product {Guid.NewGuid():N}".Substring(0, 8), 1000, 2);
        order.AddItem(Guid.NewGuid(), $"Product {Guid.NewGuid():N}".Substring(0, 8), 500, 3);

        // Act
        var total = order.TotalPrice;

        // Assert
        total.Should().Be(3500);
    }

    [Fact]
    public void Adding_OrderItem_Should_Update_TotalPrice()
    {
        // Arrange
        var order =  new OrderBuilder().Build();
        order.AddItem(Guid.NewGuid(), $"Product {Guid.NewGuid():N}".Substring(0, 8), 1000, 3);

        // Assert
        order.TotalPrice.Should().Be(3000);
    }

    [Fact]
    public void Pay_Should_Change_State_To_Paid()
    {
        // Arrange
        var order = new OrderBuilder().Build();

        // Act
        order.Pay();

        // Assert
        order.State.Should().Be(OrderState.Paid);
    }

    [Fact]
    public void Create_Should_Raise_OrderInitiatedDomainEvent()
    {
        var order = Order.Create(Guid.NewGuid());

        order.DomainEvents.Should().ContainSingle(e => e is OrderInitiatedDomainEvent);
    }
    
    [Fact]
    public void AddItem_Should_Add_Item_To_Order()
    {
        // Arrange
        var order = new OrderBuilder().Build();

        // Act
        order.AddItem(Guid.NewGuid(), "Product", 1000, 2);

        // Assert
        order.Items.Should().HaveCount(1);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AddItem_With_Invalid_Quantity_Should_Throw(int quantity)
    {
        // Arrange
        var order = new OrderBuilder().Build();

        // Act
        var act = () => order.AddItem(Guid.NewGuid(), "Product", 1000, quantity);

        // Assert
        act.Should().Throw<DomainException>();
    }
}