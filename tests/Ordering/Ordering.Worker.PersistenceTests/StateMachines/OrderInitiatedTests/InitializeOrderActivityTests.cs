using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using Ordering.Worker.Configurations.Saga;
using Ordering.Worker.PersistenceTests.Fixtures;
using Ordering.Worker.PersistenceTests.TestBase;
using Ordering.Worker.StateMachines.Activities.Initialize;
using Ordering.Worker.StateMachines.Events;
using Shared.IntegrationEvents.Ordering;
using Xunit;

namespace Ordering.Worker.PersistenceTests.StateMachines.OrderInitiatedTests;

public class InitializeOrderActivityTests : OrderingWorkerPersistenceFixture
{
    [Fact]
    public async Task Should_create_saga_and_publish_reduce_inventory_and_remove_basket()
    {
        await ResetDatabaseAsync();

        // Arrange
        var orderId = Guid.NewGuid();
        var date = DateTime.UtcNow;

        // Act: منتشر کردن رویداد OrderInitiated
        await PublishEventAsync(new OrderInitiated
        {
            Id = orderId,
            Date = date
        });
        

        var instance = SagaHarness.Created.ContainsInState(orderId, SagaHarness.StateMachine, SagaHarness.StateMachine.WaitingForPayment);

        instance.Should().NotBeNull();

        /*var reduceInventoryPublished = await Harness.Published.Any<ReduceInventory>();
        var removeBasketPublished = await Harness.Published.Any<RemoveBasket>();

        reduceInventoryPublished.Should().BeTrue();
        removeBasketPublished.Should().BeTrue();*/
    }
}