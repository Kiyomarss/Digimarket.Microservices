using AutoFixture;
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
    
    [Theory]
    [InlineData("Shipped", 300_000L)]
    [InlineData("Processing", 250_000L)]
    [InlineData("Pending", 200_000L)]
    public async Task Handle_Should_Return_Correct_TotalAmount_For_Given_State(string state, long expectedTotal)
    {
        await CleanupDatabase();

        DbContext.Orders.AddRange(
                                  new OrderBuilder().WithState("Processing")
                                                    .WithItems((TestGuids.Guid5, 2, 100_000L), (TestGuids.Guid4, 1, 50_000L))
                                                    .Build(),

                                  new OrderBuilder().WithState("Shipped")
                                                    .WithItems((TestGuids.Guid4, 3, 100_000L))
                                                    .Build(),

                                  new OrderBuilder().WithState("Pending")
                                                    .WithItems((TestGuids.Guid3, 1, 200_000L))
                                                    .Build()
                                 );

        await DbContext.SaveChangesAsync();

        var query = new GetCurrentUserOrdersQuery(state);
        var result = await Sender.Send(query);

        result.Orders.Should().ContainSingle();
        result.Orders[0].TotalPrice.Should().Be(expectedTotal);
    }
}