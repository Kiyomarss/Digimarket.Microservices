using FluentAssertions;
using Ordering_Domain.Domain.Enum;
using Ordering.Application.Orders.Queries;
using Ordering.TestingInfrastructure.Builders;
using Ordering.TestingInfrastructure.Fixtures;
using Ordering.TestingInfrastructure.TestBase;

namespace Ordering.Application.IntegrationTests.Orders.Queries.GetCurrentUserOrders;

public class GetCurrentUserOrdersHandler : OrderingAppTestBase
{
    public GetCurrentUserOrdersHandler(OrderingAppFactory fixture)
        : base(fixture) { }

    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_No_Orders_Exist()
    {
        await ResetDatabase();

        var result = await Sender.Send(new GetCurrentUserOrdersQuery("Pending"));

        result.Orders.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_No_Order_Matches_State()
    {
        await ResetDatabase();

        DbContext.Orders.Add(new OrderBuilder().Canceled().Build());

        await DbContext.SaveChangesAsync();

        var result = await Sender.Send(new GetCurrentUserOrdersQuery("Pending"));

        result.Orders.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_Return_All_Matching_Orders()
    {
        await ResetDatabase();

        DbContext.Orders.AddRange(
                                  new OrderBuilder().Build(),
                                  new OrderBuilder().Build()
                                 );

        await DbContext.SaveChangesAsync();

        var result = await Sender.Send(new GetCurrentUserOrdersQuery(OrderState.Pending.Code));

        result.Orders.Should().HaveCount(2);
    }
}