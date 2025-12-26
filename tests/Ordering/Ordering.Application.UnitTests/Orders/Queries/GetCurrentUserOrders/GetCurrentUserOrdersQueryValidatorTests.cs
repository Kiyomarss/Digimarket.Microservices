using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Ordering.Application.Orders.Queries;
using Ordering_Domain.Domain.Enum;
using Xunit;

namespace Ordering.Application.UnitTests.Orders.Queries.GetCurrentUserOrders;

public class GetCurrentUserOrdersQueryValidatorTests
{
    private readonly GetCurrentUserOrdersQueryValidator _validator = new();
    
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
        // Arrange
        var query = new GetCurrentUserOrdersQuery(validState);

        // Act
        ValidationResult result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
    
    [Theory]
    [InlineData("")]
     [InlineData(" ")]
    [InlineData("InvalidState")]
    public void Should_Have_Error_When_State_Is_Invalid(string invalidState)
    {
        // Arrange
        var query = new GetCurrentUserOrdersQuery(invalidState);

        // Act
        ValidationResult result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors
              .Should().Contain(e => e.PropertyName == nameof(query.State))
              .And.HaveCount(1);

        var errorMessage = result.Errors[0].ErrorMessage;
        errorMessage.Should().BeOneOf("وضعیت سفارش الزامی است.", "وضعیت سفارش نامعتبر است.");
    }
    
}