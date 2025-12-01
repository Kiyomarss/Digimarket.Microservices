using FluentAssertions;
using Ordering_Domain.Domain.Entities;
using Ordering.Application.Orders.Queries;
using Ordering.TestingInfrastructure.Fixtures;
using Ordering.TestingInfrastructure.TestBase;
using Shared;

namespace Ordering.Application.IntegrationTests.Orders.Queries.GetCurrentUserOrders;

public class GetCurrentUserOrdersHandler : OrderingAppTestBase
{
    public GetCurrentUserOrdersHandler(OrderingAppFactory fixture) 
        : base(fixture) { }
    
    [Fact]
    public async Task Handle_With_State_Filter_Should_Return_Only_Matching_Orders()
    {
        // Arrange
        await CleanupDatabase();

        var currentUserId = TestGuids.Guid3;

        DbContext.Orders.AddRange(
            new Order { Id = Guid.NewGuid(), UserId = currentUserId, State = "Processing", Customer = "a", Date = DateTime.Now},
            new Order { Id = Guid.NewGuid(), UserId = currentUserId, State = "Shipped", Customer = "a", Date = DateTime.Now},
            new Order { Id = Guid.NewGuid(), UserId = currentUserId, State = "Pending", Customer = "a", Date = DateTime.Now}
        );
        await DbContext.SaveChangesAsync();

        var query = new GetCurrentUserOrdersQuery ("Shipped");

        // Act
        var result = await Sender.Send(query);

        // Assert
        result.Orders.Should().HaveCount(1);
        result.Orders[0].Should().NotBeNull();
    }
}