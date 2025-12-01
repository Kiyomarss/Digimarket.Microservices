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
    [MemberData(nameof(OrderTestData))]
    public async Task Handle_Should_Return_Correct_TotalAmount_For_Given_State(
        string state, 
        long expectedTotal, 
        (int quantity, long price)[] items)
    {
        await CleanupDatabase();

        DbContext.Orders.Add(
                             new OrderBuilder()
                                 .WithState(state)
                                 .WithItems(items)
                                 .Build()
                            );

        // سفارش‌های دیگر برای تست فیلتر
        DbContext.Orders.AddRange(
                                  new OrderBuilder().WithState("Other").Build(),
                                  new OrderBuilder().WithState("Cancelled").Build()
                                 );

        await DbContext.SaveChangesAsync();

        var result = await Sender.Send(new GetCurrentUserOrdersQuery(state));

        result.Orders.Should().HaveCount(1);
        result.Orders[0].TotalPrice.Should().Be(expectedTotal);
    }

    public static IEnumerable<object[]> OrderTestData()
    {
        yield return ["Shipped", 300_000L, new[] { (3, 100_000L) }];
        yield return ["Processing", 250_000L, new[] { (2, 100_000L), (1, 50_000L) }];
        yield return ["Pending", 200_000L, new[] { (1, 200_000L) }];
    }
}