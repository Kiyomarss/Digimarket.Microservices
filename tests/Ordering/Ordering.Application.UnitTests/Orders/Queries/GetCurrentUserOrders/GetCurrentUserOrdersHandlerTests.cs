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
    private readonly Guid _currentUserId = TestGuids.Guid3;

    private static GetCurrentUserOrdersHandler CreateHandler(
        Mock<IOrderRepository>? orderRepo = null,
        Mock<ICurrentUserService>? user = null)
    {
        user ??= new Mock<ICurrentUserService>();
        user.Setup(x => x.GetRequiredUserId()).ReturnsAsync(TestGuids.Guid3);

        return new GetCurrentUserOrdersHandler(
            orderRepo?.Object ?? new Mock<IOrderRepository>().Object,
            user.Object);
    }

    [Fact]
    public async Task Handle_WhenStateFilterApplied_Should_ReturnOnlyMatchingOrders()
    {
        // Arrange — هر تست خودش Mock می‌سازد
        var orderRepoMock = new Mock<IOrderRepository>();

        var allOrders = new List<Order>
        {
            OrderBuilder.Shipped().WithItems((1, 100_000L)).Build(),
            OrderBuilder.Processing().WithItems((1, 50_000L)).Build(),
            OrderBuilder.Shipped().WithItems((1, 200_000L)).Build(),
            OrderBuilder.Pending().WithItems((1, 300_000L)).Build()
        };

        orderRepoMock
            .Setup(x => x.GetOrdersForUserAsync(
                _currentUserId,
                "Shipped",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(allOrders.Where(o => o.State == "Shipped").ToList());

        var handler = CreateHandler(orderRepo: orderRepoMock);
        var query = new GetCurrentUserOrdersQuery("Shipped");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Orders.Should().HaveCount(2);
        result.Orders.Should().OnlyContain(o =>
            o.TotalPrice == 100_000L || o.TotalPrice == 200_000L);
    }

    [Fact]
    public async Task Handle_WhenNoOrdersExist_Should_ReturnEmptyList()
    {
        // Arrange
        var orderRepoMock = new Mock<IOrderRepository>();
        var currentUserMock = new Mock<ICurrentUserService>();

        orderRepoMock
            .Setup(x => x.GetOrdersForUserAsync(
                _currentUserId,
                "AnyState",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Order>());

        var handler = CreateHandler(orderRepo: orderRepoMock, user: currentUserMock);
        var query = new GetCurrentUserOrdersQuery("AnyState");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Orders.Should().BeEmpty();

        currentUserMock.Verify(x => x.GetRequiredUserId(), Times.Once);
        orderRepoMock.Verify(x => x.GetOrdersForUserAsync(
            _currentUserId, "AnyState", It.IsAny<CancellationToken>()), Times.Once);
    }
}