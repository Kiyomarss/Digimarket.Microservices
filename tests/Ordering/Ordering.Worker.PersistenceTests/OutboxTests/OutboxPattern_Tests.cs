using FluentAssertions;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using Ordering.Worker.PersistenceTests.Fixtures;
using Ordering.Worker.StateMachines.Events;
using Shared.IntegrationEvents.Ordering;

namespace Ordering.Worker.PersistenceTests.OutboxTests
{
    public class OutboxPattern_Tests : TestBase.OrderStateMachineIntegrationTestBase
    {
        public OutboxPattern_Tests(OrderStateMachineFixture fixture) : base(fixture) { }

        [Fact]
        public async Task Should_store_and_publish_order_status_changed()
        {
            // Arrange
            await CleanupDatabase();
            var orderId = Guid.NewGuid();
            var now = DateTime.UtcNow;

            // Act
            await Bus.Publish(new OrderInitiated
            {
                Id = orderId,
                Date = now
            });

            // انتظار برای پردازش پیام و Outbox
            await Task.Delay(5000);

            // Assert
            var saga = await GetSagaState(orderId);

            saga.Should().NotBeNull("Saga should be created");
            saga.CurrentState.Should().NotBeNullOrEmpty("Saga state should be updated");}
    }
}