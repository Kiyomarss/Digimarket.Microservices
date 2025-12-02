using BuildingBlocks.Services;
using FluentAssertions;
using Moq;
using Ordering.Application.Orders.Queries;
using Ordering_Domain.Domain.Entities;
using Ordering_Domain.Domain.RepositoryContracts;
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
        // Arrange
        var allOrders = new List<Order>
        {
            OrderBuilder.Shipped().WithItems((1, 100_000L)).Build(),
            OrderBuilder.Processing().WithItems((1, 50_000L)).Build(),
            OrderBuilder.Shipped().WithItems((1, 200_000L)).Build(),
            OrderBuilder.Pending().WithItems((1, 300_000L)).Build()
        };

        _orderRepositoryMock
            .Setup(x => x.GetOrdersForUserAsync(
                _currentUserId,
                "Shipped",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(allOrders.Where(o => o.State == "Shipped").ToList());

        var query = new GetCurrentUserOrdersQuery("Shipped");

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
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Order>());

        var query = new GetCurrentUserOrdersQuery("AnyState");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        _currentUserServiceMock.Verify(x => x.GetRequiredUserId(), Times.Once);
        result.Orders.Should().BeEmpty();
        _orderRepositoryMock.Verify(x => x.GetOrdersForUserAsync(_currentUserId, "AnyState", It.IsAny<CancellationToken>()), Times.Once);
    }
}