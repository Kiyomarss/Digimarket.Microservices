using System;
using System.Linq;
using System.Threading.Tasks;
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
        InMemoryTestHarness Harness = null!;
        ISagaStateMachineTestHarness<OrderStateMachine, OrderState> SagaHarness = null!;

        public async Task InitializeAsync()
        {
            // create harness and enable in-memory scheduler (required for ScheduleSend)
            Harness = new InMemoryTestHarness();
            Harness.OnConfigureInMemoryBus += cfg => cfg.UseInMemoryScheduler();

            // create state machine and repository, then register saga on the harness
            var machine = new OrderStateMachine();
            var repository = new InMemorySagaRepository<OrderState>();

            // StateMachineSaga extension returns an ISagaStateMachineTestHarness
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
            var machine = SagaHarness.StateMachine; // the same instance we registered
            var orderId = Guid.NewGuid();
            var now = DateTime.UtcNow;

            // Act: publish the initiating event
            await Harness.Bus.Publish(new OrderInitiated
            {
                Id = orderId,
                Customer = "TestCustomer",
                Date = now
            });

            // === Correct way to wait for saga existence AND state ===
            // Wait until an instance with correlationId exists in WaitingForPayment
            var sagaInstanceId = await SagaHarness.Exists(orderId, x => x.WaitingForPayment);
            Assert.NotNull(sagaInstanceId); // fail early if not created

            // Now get the actual instance from the Created collection
            var instance = SagaHarness.Created.Contains(orderId);
            Assert.NotNull(instance);

            // Assert: state
            Assert.Equal(machine.WaitingForPayment.Name, instance.CurrentState);

            // Assert: instance properties set by the state machine
            Assert.Equal(now, instance.Date);
            Assert.Equal("TestCustomer", instance.Customer);

            // Assert: published messages (ReduceInventory and RemoveBasket)
            Assert.True(await Harness.Published.Any<ReduceInventory>(x => x.Context.Message.Id == orderId),
                "ReduceInventory should be published");
            Assert.True(await Harness.Published.Any<RemoveBasket>(x => x.Context.Message.Id == orderId),
                "RemoveBasket should be published");

            // Assert: OrderStatusChanged published with expected state
            Assert.True(await Harness.Published.Any<OrderStatusChanged>(x =>
                x.Context.Message.Id == orderId &&
                x.Context.Message.OrderState == machine.WaitingForPayment.Name),
                "OrderStatusChanged should be published with WaitingForPayment");

            // Assert: scheduled messages were sent (in-memory scheduler stores sent scheduled messages)
            // Scheduled messages show up in Sent as ScheduledMessage<T>
            Assert.True(Harness.Sent.Select<ScheduledMessage<SendReminder>>().Any(),
                "SendReminder should be scheduled (found in Sent as ScheduledMessage<SendReminder>)");
            Assert.True(Harness.Sent.Select<ScheduledMessage<CancelOrder>>().Any(),
                "CancelOrder should be scheduled (found in Sent as ScheduledMessage<CancelOrder>)");

            // Also ensure saga stored the schedule token ids (non-null)
            Assert.NotNull(instance.ReminderScheduleTokenId);
            Assert.NotNull(instance.CancelScheduleTokenId);
        }
    }
}