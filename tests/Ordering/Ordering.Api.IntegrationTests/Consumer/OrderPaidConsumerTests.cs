using FluentAssertions;
using Ordering_Domain.Domain.Enum;
using Ordering.TestingInfrastructure.Fixtures;
using Ordering.TestingInfrastructure.TestBase;
using Shared.IntegrationEvents.Ordering;

namespace Ordering.Api.IntegrationTests.Consumer;

public class OrderPaidConsumerTests : OrderingAppTestBase
{
    public OrderPaidConsumerTests(OrderingAppFactory fixture)
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
        
        await PublishEventAsync(new OrderPaid
        {
            Id = order.Id
        });
        
        // صبر کن تا مصرف پیام تمام شود
        (await Harness.Consumed.Any<OrderPaid>()).Should().BeTrue();
        
        await ReloadEntityAsync(order);

        order.State.Should().Be(OrderState.Paid);
    }
}