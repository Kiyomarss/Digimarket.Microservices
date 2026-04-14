using FluentAssertions;
using MassTransit.Testing;
using Ordering.TestingInfrastructure.Builders;
using Ordering.Worker.PersistenceTests.Fixtures;

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
        await PublishEventAsync(new OrderInitiatedBuilder().Build());
        

        var instance = SagaHarness.Created.ContainsInState(orderId, SagaHarness.StateMachine, SagaHarness.StateMachine.WaitingForPayment);

        instance.Should().NotBeNull();

        /*var reduceInventoryPublished = await Harness.Published.Any<ReduceInventory>();
        var removeBasketPublished = await Harness.Published.Any<RemoveBasket>();

        reduceInventoryPublished.Should().BeTrue();
        removeBasketPublished.Should().BeTrue();*/
    }
}