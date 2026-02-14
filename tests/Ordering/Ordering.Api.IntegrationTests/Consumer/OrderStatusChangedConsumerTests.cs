using FluentAssertions;
using Ordering_Domain.Domain.Enum;
using Ordering.TestingInfrastructure.Fixtures;
using Ordering.TestingInfrastructure.TestBase;
using Shared.IntegrationEvents.Ordering;

namespace Ordering.Api.IntegrationTests.Consumer;

public class OrderStatusChangedConsumerTests : OrderingAppTestBase
{
    public OrderStatusChangedConsumerTests(OrderingAppFactory fixture)
        : base(fixture) { }

    [Fact]
    public async Task Publish_OrderPaid_Event()
    {
        await ResetDatabase();
        
        var order = new OrderBuilder()
                    .WithItems((1, 100))
                    .Build();

        DbContext.Orders.Add(order);
        await DbContext.SaveChangesAsync();

        await Harness.Bus.Publish(new OrderPaid
        {
            Id = order.Id
        });
        
        var orderAfter = await DbContext.Orders.FindAsync(order.Id);
        orderAfter!.State.Should().Be(OrderState.Paid);
    }
}