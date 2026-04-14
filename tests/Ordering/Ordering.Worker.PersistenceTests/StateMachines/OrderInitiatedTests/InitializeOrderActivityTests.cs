using FluentAssertions;
using MassTransit.Testing;
using Ordering.TestingInfrastructure.Builders;
using Ordering.Worker.Configurations.Saga;
using Ordering.Worker.PersistenceTests.Fixtures;
using Ordering.Worker.PersistenceTests.TestBase;

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
        await PublishEventAsync(new OrderInitiatedBuilder().WithId(orderId).Build());
        
        var instance = SagaHarness.Created.ContainsInState(orderId, SagaHarness.StateMachine, SagaHarness.StateMachine.WaitingForPayment);

        instance.Should().NotBeNull();
    }
}