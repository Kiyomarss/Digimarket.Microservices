using FluentAssertions;
using Ordering_Domain.Domain.Entities;

namespace Ordering.Domain.UnitTests.Entities;

public class OrderItemTests
{
    [Fact]
    public void Parameterless_Constructor_Should_Create_Instance_For_EF()
    {
        // Act
        var item = new OrderItem();

        // Assert
        item.Id.Should().NotBe(Guid.Empty);
        item.Price.Should().Be(0);
        item.Quantity.Should().Be(0);
        item.ProductName.Should().BeNullOrEmpty();
    }

    [Fact]
    public void Constructor_With_Parameters_Should_Set_Properties()
    {
        // Arrange
        var productId = Guid.NewGuid();

        // Act
        var item = new OrderItem(productId, "Test Product", 5, 999);

        // Assert
        item.ProductId.Should().Be(productId);
        item.ProductName.Should().Be("Test Product");
        item.Quantity.Should().Be(5);
        item.Price.Should().Be(999);
    }
}