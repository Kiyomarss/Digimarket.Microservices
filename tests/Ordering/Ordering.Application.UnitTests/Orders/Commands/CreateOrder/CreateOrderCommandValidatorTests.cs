using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Ordering.Application.Orders.Commands.CreateOrder;
using Xunit;

namespace Ordering.Application.UnitTests.Orders.Commands.CreateOrder;

public class CreateOrderCommandValidatorTests
{
    private readonly CreateOrderCommandValidator _validator = new();


    [Fact]
    public void Should_Have_Error_When_Items_Is_Empty()
    {
        // Arrange
        var command = new CreateOrderCommand { };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.Errors
              .Should().ContainSingle(e => e.PropertyName == nameof(command.Items))
              .Which.ErrorMessage.Should().Be("Order must contain at least one item.");
    }

    [Fact]
    public void Should_Have_Error_When_ProductId_Is_Not_Valid_Guid()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            Items = new List<CreateOrderCommand.OrderItemDto>
            {
                new() { ProductId = "invalid-guid", Quantity = 1 }
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.Errors
              .Should().ContainSingle(e => e.PropertyName == "Items[0].ProductId")
              .Which.ErrorMessage.Should().Be("ProductId must be a valid GUID.");
    }

    [Fact]
    public void Should_Have_Error_When_Duplicate_ProductId_Exists()
    {
        // Arrange
        var productId = Guid.NewGuid().ToString();
        var command = new CreateOrderCommand
        {
            Items = new List<CreateOrderCommand.OrderItemDto>
            {
                new() { ProductId = productId, Quantity = 1 },
                new() { ProductId = productId, Quantity = 2 }
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.Errors
              .Should().ContainSingle(e => e.PropertyName == nameof(command.Items))
              .Which.ErrorMessage.Should().Be("Duplicate products are not allowed in the order.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            Items = new List<CreateOrderCommand.OrderItemDto>
            {
                new() { ProductId = Guid.NewGuid().ToString(), Quantity = 5 },
                new() { ProductId = Guid.NewGuid().ToString(), Quantity = 10 }
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}