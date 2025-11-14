using FluentAssertions;
using Shared.IntegrationEvents.Ordering;

namespace Ordering.Worker.IntegrationTests.ConsumerTests
{
    public class OrderInitiatedConsumer_Tests : TestBase.OrderStateMachineIntegrationTestBase
    {
        private readonly Fixtures.OrderStateMachineFixture _fixture;

        public OrderInitiatedConsumer_Tests(Fixtures.OrderStateMachineFixture fixture) : base()
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Should_consume_order_initiated_message()
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

            // انتظار برای پردازش پیام
            await Task.Delay(1000);

            // Assert
            var saga = await GetSagaState(orderId);
            saga.Should().NotBeNull("Saga should be created");
            saga!.CurrentState.Should().Be("WaitingForPayment");
            saga.Date.Should().Be(now);
            saga.Customer.Should().Be("TestCustomer");
            saga.ReminderScheduleTokenId.Should().NotBeNull("Reminder should be scheduled");
            saga.CancelScheduleTokenId.Should().NotBeNull("CancelOrder should be scheduled");
        }
    }
}