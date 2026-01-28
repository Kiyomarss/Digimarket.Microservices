using BuildingBlocks.Services;
using FluentAssertions;
using Moq;
using Ordering.Application.Orders.Queries;
using Ordering_Domain.Domain.Entities;
using Ordering_Domain.Domain.Enum;
using Ordering.Application.RepositoryContracts;
using Shared;

namespace Ordering.Application.UnitTests.Orders.Queries.GetCurrentUserOrders;

public class GetCurrentUserOrdersHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly GetCurrentUserOrdersHandler _handler;

    private readonly Guid _currentUserId = TestGuids.Guid3;

    public GetCurrentUserOrdersHandlerTests()
    {
        _currentUserServiceMock
            .Setup(x => x.GetRequiredUserId())
            .ReturnsAsync(_currentUserId);
        
        _handler = new GetCurrentUserOrdersHandler(
            _orderRepositoryMock.Object,
            _currentUserServiceMock.Object);
    }
    
    [Fact]
    public async Task Handle_WhenStateFilterApplied_Should_ReturnOnlyMatchingOrders()
    {
        var summaries = new List<OrderSummaryDto>
        {
            new(DateTime.UtcNow, 100_000L), new(DateTime.UtcNow, 200_000L)
        };

        _orderRepositoryMock
            .Setup(x => x.GetOrdersForUserAsync(
                                                _currentUserId,
                                                OrderState.Shipped,
                                                It.IsAny<CancellationToken>()))
            .ReturnsAsync(summaries);

        var query = new GetCurrentUserOrdersQuery(OrderState.Shipped.Code);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Orders.Should().HaveCount(2);
        result.Orders.Should().OnlyContain(o =>
            o.TotalPrice == 100_000L || o.TotalPrice == 200_000L);
    }

    [Fact]
    public async Task Handle_WhenNoOrdersExist_Should_ReturnEmptyList()
    {
        // Arrange
        _orderRepositoryMock
            .Setup(x => x.GetOrdersForUserAsync(
                It.IsAny<Guid>(),
                It.IsAny<OrderState>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OrderSummaryDto>());

        var query = new GetCurrentUserOrdersQuery(OrderState.Shipped.Code);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        _currentUserServiceMock.Verify(x => x.GetRequiredUserId(), Times.Once);
        result.Orders.Should().BeEmpty();
        _orderRepositoryMock.Verify(x => x.GetOrdersForUserAsync(_currentUserId, OrderState.Paid, It.IsAny<CancellationToken>()), Times.Once);
    }
}