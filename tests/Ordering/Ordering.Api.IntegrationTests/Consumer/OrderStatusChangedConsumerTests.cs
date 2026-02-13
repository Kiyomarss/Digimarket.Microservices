using Ordering.TestingInfrastructure.Fixtures;
using Ordering.TestingInfrastructure.TestBase;
using Shared;
using Shared.IntegrationEvents.Ordering;

namespace Ordering.Api.IntegrationTests.Consumer;

public class OrderStatusChangedConsumerTests : OrderingAppTestBase
{
    public OrderStatusChangedConsumerTests(OrderingAppFactory fixture)
        : base(fixture) { }

    [Fact]
    public async Task CreateOrder_Should_Publish_OrderInitiated_Event()
    {
        await ResetDatabase();

        await Harness.Bus.Publish(new OrderPaid { Id = TestGuids.Guid3 });

        Assert.True(await Harness.Consumed.Any<OrderPaid>(), "OrderPaid event not consumed");
    }
}