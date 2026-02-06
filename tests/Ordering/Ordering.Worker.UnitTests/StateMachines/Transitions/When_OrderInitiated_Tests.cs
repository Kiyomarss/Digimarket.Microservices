using FluentAssertions;
using Ordering.Worker.StateMachines.Events;
using Ordering.Worker.UnitTests.StateMachines.Fixtures;
using Ordering.Worker.UnitTests.StateMachines.TestBases;
using Shared.IntegrationEvents.Ordering;

namespace Ordering.Worker.UnitTests.StateMachines.Transitions
{
    public class When_OrderInitiated_Tests : OrderStateMachineTestBase
    {
        public When_OrderInitiated_Tests(OrderStateMachineFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async Task Should_create_saga_and_schedule_messages_and_publish_events()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var now = DateTime.UtcNow;

            // Act: انتشار OrderInitiated
            await Harness.Bus.Publish(new OrderInitiated
            {
                Id = orderId,
                Date = now
            });

            // Assert: انتظار تا saga ایجاد و وارد وضعیت WaitingForPayment شود
            var sagaInstanceId = await SagaHarness.Exists(orderId, x => x.WaitingForPayment);
            sagaInstanceId.Should().NotBeNull("saga instance should exist in WaitingForPayment state");

            var instance = SagaHarness.Created.Contains(orderId);
            instance.Should().NotBeNull("saga instance should be created");

            // بررسی وضعیت saga
            instance.CurrentState.Should().Be(Machine.WaitingForPayment.Name);
            instance.Date.Should().Be(now);

            // بررسی پیام‌های منتشرشده
            (await Harness.Published.Any<ReduceInventory>(x => x.Context.Message.Id == orderId))
                .Should().BeTrue("ReduceInventory should be published");

            (await Harness.Published.Any<RemoveBasket>(x => x.Context.Message.Id == orderId))
                .Should().BeTrue("RemoveBasket should be published");

            (await Harness.Published.Any<OrderStatusChanged>(x =>
                    x.Context.Message.Id == orderId &&
                    x.Context.Message.State == Machine.WaitingForPayment.Name))
                .Should().BeTrue("OrderStatusChanged should be published with WaitingForPayment");

            // بررسی TokenIdهای زمان‌بندی‌شده
            instance.ReminderScheduleTokenId.Should().NotBeNull("Reminder should be scheduled");
            instance.CancelScheduleTokenId.Should().NotBeNull("CancelOrder should be scheduled");
        }
    }
}