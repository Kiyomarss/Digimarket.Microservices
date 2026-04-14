using BuildingBlocks.IntegrationEvents;
using FluentAssertions;
using Ordering_Domain.Domain.Enum;
using Ordering.TestingInfrastructure.Builders;
using Ordering.TestingInfrastructure.Fixtures;
using Ordering.TestingInfrastructure.TestBase;

namespace Ordering.Application.IntegrationTests.Orders.Consumer;

public class OrderCanceledConsumerTests : OrderingAppTestBase
{
    public OrderCanceledConsumerTests(OrderingAppFactory fixture)
        : base(fixture) { }

    [Fact]
    public async Task Publish_OrderCanceled_Event()
    {
        await ResetDatabase();
        
        var order = new OrderBuilder().Build();

        DbContext.Orders.Add(order);
        await DbContext.SaveChangesAsync();
        
        await PublishEventAsync(new OrderCanceled(order.Id));
        
        await AssertPublishedAsync<OrderCanceled>();
        await AssertConsumedAsync<OrderCanceled>();
        
        await ReloadEntityAsync(order);

        order.State.Should().Be(OrderState.Canceled);
    }
}