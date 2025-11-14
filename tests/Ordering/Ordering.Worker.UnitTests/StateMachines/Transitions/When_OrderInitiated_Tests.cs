using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using Ordering.Worker.StateMachines;
using Ordering.Worker.StateMachines.Events;
using Ordering.Worker.Configurations.Saga;
using Shared.IntegrationEvents.Ordering;

namespace Ordering.Worker.UnitTests.StateMachines.Transitions
{
    public class When_OrderInitiated_Tests : IAsyncLifetime
    {
        private InMemoryTestHarness Harness = null!;
        private ISagaStateMachineTestHarness<OrderStateMachine, OrderState> SagaHarness = null!;

        public async Task InitializeAsync()
        {
            Harness = new InMemoryTestHarness();

            // تنظیم Message Scheduler برای تست پیام‌های زمان‌بندی‌شده
            Harness.OnConfigureInMemoryBus += configurator =>
            {
                configurator.UseMessageScheduler(new Uri("queue:quartz"));
            };

            var machine = new OrderStateMachine();
            var repository = new InMemorySagaRepository<OrderState>();

            SagaHarness = Harness.StateMachineSaga(machine, repository);

            await Harness.Start();
        }

        public async Task DisposeAsync()
        {
            if (Harness != null)
                await Harness.Stop();
        }

        [Fact]
        public async Task Should_create_saga_and_schedule_messages_and_publish_events()
        {
            // Arrange
            var machine = SagaHarness.StateMachine;
            var orderId = Guid.NewGuid();
            var now = DateTime.UtcNow;

            // Act: انتشار OrderInitiated
            await Harness.Bus.Publish(new OrderInitiated
            {
                Id = orderId,
                Customer = "TestCustomer",
                Date = now
            });

            // انتظار تا saga ایجاد و وارد وضعیت WaitingForPayment شود
            var sagaInstanceId = await SagaHarness.Exists(orderId, x => x.WaitingForPayment);
            sagaInstanceId.Should().NotBeNull("saga instance should exist in WaitingForPayment state");

            var instance = SagaHarness.Created.Contains(orderId);
            instance.Should().NotBeNull("saga instance should be created");

            // بررسی وضعیت saga
            instance.CurrentState.Should().Be(machine.WaitingForPayment.Name);
            instance.Date.Should().Be(now);
            instance.Customer.Should().Be("TestCustomer");

            // بررسی پیام‌های منتشرشده
            (await Harness.Published.Any<ReduceInventory>(x => x.Context.Message.Id == orderId))
                .Should().BeTrue("ReduceInventory should be published");

            (await Harness.Published.Any<RemoveBasket>(x => x.Context.Message.Id == orderId))
                .Should().BeTrue("RemoveBasket should be published");

            (await Harness.Published.Any<OrderStatusChanged>(x =>
                    x.Context.Message.Id == orderId &&
                    x.Context.Message.OrderState == machine.WaitingForPayment.Name))
                .Should().BeTrue("OrderStatusChanged should be published with WaitingForPayment");

            // بررسی TokenIdهای زمان‌بندی‌شده
            instance.ReminderScheduleTokenId.Should().NotBeNull("Reminder should be scheduled");
            instance.CancelScheduleTokenId.Should().NotBeNull("CancelOrder should be scheduled");
        }
    }
}