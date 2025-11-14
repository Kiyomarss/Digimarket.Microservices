using FluentAssertions;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using Ordering.Worker.StateMachines.Events;
using Shared.IntegrationEvents.Ordering;

namespace Ordering.Worker.IntegrationTests.OutboxTests
{
    public class OutboxPattern_Tests : TestBase.OrderStateMachineIntegrationTestBase
    {
        public OutboxPattern_Tests(Fixtures.OrderStateMachineFixture fixture) : base(fixture)
        {
        }

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
                Customer = "TestCustomer",
                Date = now
            });

            // انتظار برای پردازش پیام و Outbox
            await Task.Delay(2000);

            // Assert
            var saga = await GetSagaState(orderId);
            saga.Should().NotBeNull("Saga should be created");

            // بررسی پیام‌های Outbox
            var outboxMessages = await DbContext.Set<OutboxMessage>()
                                                .Where(m => m.MessageType.Contains(nameof(OrderStatusChanged)))
                                                .ToListAsync();

            outboxMessages.Should().NotBeEmpty("OrderStatusChanged should be stored in Outbox");
            outboxMessages.Should().Contain(m => m.Body.Contains(orderId.ToString()),
                                            "OrderStatusChanged should contain the correct OrderId");
        }
    }
}