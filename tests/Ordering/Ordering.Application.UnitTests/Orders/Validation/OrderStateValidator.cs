using FluentAssertions;
using Ordering.Application.Orders.Queries;
using Ordering.Application.Orders.Validation;

namespace Ordering.Application.UnitTests.Orders.Validation;

public class OrderStateValidatorTests
{
    private readonly OrderStateValidator _validator = new();

    [Theory]
    [InlineData("pending")]
    [InlineData("Paid")]
    [InlineData("shipped")]
    [InlineData("Processing")]
    [InlineData("Delivered")]
    [InlineData("Cancelled")]
    [InlineData("Returned")]
    public void Should_Not_Have_Error_When_State_Is_Valid(string validState)
    {
        // Act
        var result = _validator.Validate(validState);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Return_Required_Error_When_State_Is_Empty_Or_Whitespace(string emptyState)
    {
        var result = _validator.Validate(emptyState);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
              .Which.ErrorMessage.Should().Be("وضعیت سفارش الزامی است.");
    }

    [Theory]
    [InlineData("InvalidState")]
    [InlineData("unknown")]
    [InlineData("abc123")]
    public void Should_Return_Invalid_Error_When_State_Is_Not_Recognized(string invalidState)
    {
        var result = _validator.Validate(invalidState);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
              .Which.ErrorMessage.Should().Be("وضعیت سفارش نامعتبر است.");
    }
}