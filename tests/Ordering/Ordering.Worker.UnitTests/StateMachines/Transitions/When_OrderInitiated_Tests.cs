using System;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Testing;
using Ordering.Worker.StateMachines;
using Ordering.Worker.StateMachines.Events;
using Ordering.Worker.Configurations.Saga;
using Shared.IntegrationEvents.Ordering;
using Xunit;

namespace Ordering.Worker.UnitTests.StateMachines.Transitions
{
    public class When_OrderInitiated_Tests : IAsyncLifetime
    {
        InMemoryTestHarness Harness = null!;
        ISagaStateMachineTestHarness<OrderStateMachine, OrderState> SagaHarness = null!;

        public async Task InitializeAsync()
        {
            Harness = new InMemoryTestHarness();

            // تنظیم Message Scheduler برای تست پیام‌های زمان‌بندی‌شده
            Harness.OnConfigureInMemoryBus += configurator =>
            {
                // استفاده از InMemory Message Scheduler به جای Quartz برای تست
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
            Assert.NotNull(sagaInstanceId);

            var instance = SagaHarness.Created.Contains(orderId);
            Assert.NotNull(instance);

            // بررسی وضعیت saga
            Assert.Equal(machine.WaitingForPayment.Name, instance.CurrentState);
            Assert.Equal(now, instance.Date);
            Assert.Equal("TestCustomer", instance.Customer);

            // بررسی پیام‌های منتشرشده
            Assert.True(await Harness.Published.Any<ReduceInventory>(x => x.Context.Message.Id == orderId),
                "ReduceInventory should be published");
            Assert.True(await Harness.Published.Any<RemoveBasket>(x => x.Context.Message.Id == orderId),
                "RemoveBasket should be published");
            Assert.True(await Harness.Published.Any<OrderStatusChanged>(x =>
                x.Context.Message.Id == orderId &&
                x.Context.Message.OrderState == machine.WaitingForPayment.Name),
                "OrderStatusChanged should be published with WaitingForPayment");

            // بررسی TokenIdهای زمان‌بندی‌شده
            Assert.NotNull(instance.ReminderScheduleTokenId);
            Assert.NotNull(instance.CancelScheduleTokenId);

            // بررسی پیام‌های زمان‌بندی‌شده با استفاده از MessageSchedulerContext
            var scheduler = Harness.Bus; // مستقیماً از IBus استفاده می‌کنیم
            var reminderScheduled = await scheduler.GetScheduledMessages();
            var reminderMessage = reminderScheduled.FirstOrDefault(x => 
                x.TokenId == instance.ReminderScheduleTokenId.Value && x.MessageObject is SendReminder);
            var cancelMessage = reminderScheduled.FirstOrDefault(x => 
                x.TokenId == instance.CancelScheduleTokenId.Value && x.MessageObject is CancelOrder);

            Assert.NotNull(reminderMessage);
            Assert.NotNull(cancelMessage);

            // بررسی زمان تقریبی زمان‌بندی
            var expectedReminderTime = now.AddMinutes(1);
            var tolerance = TimeSpan.FromSeconds(5);
            Assert.True(Math.Abs((reminderMessage.ScheduledTime.UtcDateTime - expectedReminderTime).TotalSeconds) < tolerance.TotalSeconds,
                "Reminder should be scheduled for approximately 1 minute later");
        }
    }
}