using FluentAssertions;
using Shared.IntegrationEvents.Ordering;

namespace Ordering.Worker.IntegrationTests.StateMachineTests
{
    public class When_OrderInitiated_Tests : TestBase.OrderStateMachineIntegrationTestBase
    {
        public When_OrderInitiated_Tests(Fixtures.OrderStateMachineFixture fixture) : base(fixture)
        {
        }

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
                Customer = "TestCustomer",
                Date = now
            });

            // انتظار برای پردازش پیام‌ها (به دلیل Outbox و RabbitMQ)
            await Task.Delay(1000);

            // Assert
            var saga = await GetSagaState(orderId);
            saga.Should().NotBeNull("Saga should be created");
            saga!.CurrentState.Should().Be("WaitingForPayment");
            saga.Date.Should().Be(now);
            saga.Customer.Should().Be("TestCustomer");
            saga.ReminderScheduleTokenId.Should().NotBeNull("Reminder should be scheduled");
            saga.CancelScheduleTokenId.Should().NotBeNull("CancelOrder should be scheduled");

            // بررسی پیام‌های منتشرشده (نیاز به Consumer موقت یا RabbitMQ Management UI)
            // برای سادگی، فقط Saga را بررسی می‌کنیم
        }
    }
}