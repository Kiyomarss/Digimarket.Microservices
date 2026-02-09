using AutoFixture;
using FluentAssertions;
using Ordering_Domain.Domain.Entities;
using Ordering_Domain.Domain.Enum;
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
    public async Task Handle_Should_Return_Empty_List_When_No_Orders_Exist()
    {
        await CleanupDatabase();

        var result = await Sender.Send(new GetCurrentUserOrdersQuery("Pending"));

        result.Orders.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_No_Order_Matches_State()
    {
        await CleanupDatabase();

        DbContext.Orders.Add(
                             new OrderBuilder().WithState(OrderState.Cancelled).WithItems((1, 100)).Build()
                            );

        await DbContext.SaveChangesAsync();

        var result = await Sender.Send(new GetCurrentUserOrdersQuery("Pending"));

        result.Orders.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_Return_All_Matching_Orders()
    {
        await CleanupDatabase();

        DbContext.Orders.AddRange(
                                  new OrderBuilder().WithState(OrderState.Pending).WithItems((1, 100)).Build(),
                                  new OrderBuilder().WithState(OrderState.Pending).WithItems((2, 100)).Build()
                                 );

        await DbContext.SaveChangesAsync();

        var result = await Sender.Send(new GetCurrentUserOrdersQuery("Pending"));

        result.Orders.Should().HaveCount(2);
    }
}