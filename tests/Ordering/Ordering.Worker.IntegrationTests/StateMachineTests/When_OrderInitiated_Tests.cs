using System;
using System.Threading.Tasks;
using FluentAssertions;
using Ordering.Worker.Configurations.Saga;
using Ordering.Worker.IntegrationTests.Fixtures;
using Shared.IntegrationEvents.Ordering;
using Xunit;

namespace Ordering.Worker.IntegrationTests.StateMachineTests
{
    [Collection("DatabaseCollection")]
    public class When_OrderInitiated_Tests : TestBase.OrderStateMachineIntegrationTestBase
    {
        public When_OrderInitiated_Tests(OrderStateMachineFixture fixture) : base(fixture) { }

        [Fact]
        public async Task Should_create_saga_and_publish_messages()
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

            // انتظار برای پردازش پیام‌ها (به دلیل Outbox و RabbitMQ)
            await Task.Delay(8000);

            // Assert
            var saga = await GetSagaState(orderId);
            saga.Should().NotBeNull("Saga should be created");
            saga!.CurrentState.Should().Be("WaitingForPayment");
            saga.ReminderScheduleTokenId.Should().NotBeNull("Reminder should be scheduled");
            saga.CancelScheduleTokenId.Should().NotBeNull("CancelOrder should be scheduled");
        }
    }
}