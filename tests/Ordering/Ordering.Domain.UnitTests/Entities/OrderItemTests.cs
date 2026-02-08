using FluentAssertions;
using Ordering_Domain.Domain.Entities;

namespace Ordering.Domain.UnitTests.Entities;

public class OrderItemTests
{
    [Fact]
    public void Constructor_Should_Initialize_Properties()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var productName = "Test Product";
        var quantity = 3;
        var price = 1000;

        // Act
        var item = new OrderItem(productId, productName, quantity, price);

        // Assert
        item.Id.Should().NotBe(Guid.Empty);
        item.ProductId.Should().Be(productId);
        item.ProductName.Should().Be(productName);
        item.Quantity.Should().Be(quantity);
        item.Price.Should().Be(price);
    }

    [Fact]
    public void Create_Should_Return_Valid_OrderItem()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var productName = "Test Product";
        var quantity = 2;
        var price = 500;

        // Act
        var item = OrderItem.Create(orderId, productId, productName, price, quantity);

        // Assert
        item.Id.Should().NotBe(Guid.Empty);
        item.OrderId.Should().Be(orderId);
        item.ProductId.Should().Be(productId);
        item.ProductName.Should().Be(productName);
        item.Quantity.Should().Be(quantity);
        item.Price.Should().Be(price);
    }
}