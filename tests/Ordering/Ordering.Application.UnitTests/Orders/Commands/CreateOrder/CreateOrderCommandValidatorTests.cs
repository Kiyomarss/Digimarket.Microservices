using FluentValidation.TestHelper;
using Ordering.Application.Orders.Commands.CreateOrder;

namespace Ordering.Application.UnitTests.Orders.Commands.CreateOrder;

public class CreateOrderCommandValidatorTests
{
    private readonly CreateOrderCommandValidator _validator;

    public CreateOrderCommandValidatorTests()
    {
        _validator = new CreateOrderCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Customer_Is_Empty()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            Customer = "",
            Items = new List<CreateOrderCommand.OrderItemDto>
            {
                new() { ProductId = Guid.NewGuid().ToString(), Quantity = 1 }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Customer)
              .WithErrorMessage("Customer name is required.");
    }

    [Fact]
    public void Should_Have_Error_When_Items_Is_Empty()
    {
        var command = new CreateOrderCommand
        {
            Customer = "John Doe",
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Items)
              .WithErrorMessage("Order must contain at least one item.");
    }

    [Fact]
    public void Should_Have_Error_When_ProductId_Is_Not_Valid_Guid()
    {
        var command = new CreateOrderCommand
        {
            Customer = "John Doe",
            Items = new List<CreateOrderCommand.OrderItemDto>
            {
                new() { ProductId = "invalid-guid", Quantity = 1 }
            }
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Items[0].ProductId")
              .WithErrorMessage("ProductId must be a valid GUID.");
    }

    [Fact]
    public void Should_Have_Error_When_Duplicate_ProductId_Exists()
    {
        var productId = Guid.NewGuid().ToString();
        var command = new CreateOrderCommand
        {
            Customer = "John Doe",
            Items = new List<CreateOrderCommand.OrderItemDto>
            {
                new() { ProductId = productId, Quantity = 1 },
                new() { ProductId = productId, Quantity = 2 }
            }
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Items)
              .WithErrorMessage("Duplicate products are not allowed in the order.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        var command = new CreateOrderCommand
        {
            Customer = "John Doe",
            Items = new List<CreateOrderCommand.OrderItemDto>
            {
                new() { ProductId = Guid.NewGuid().ToString(), Quantity = 5 },
                new() { ProductId = Guid.NewGuid().ToString(), Quantity = 10 }
            }
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}