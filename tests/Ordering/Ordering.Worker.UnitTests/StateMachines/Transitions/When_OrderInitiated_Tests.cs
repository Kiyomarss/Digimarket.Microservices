using MassTransit;
using MassTransit.Testing;
using Ordering.Worker.StateMachines;
using Ordering.Worker.StateMachines.Events;
using Ordering.Worker.Configurations.Saga;
using Shared.IntegrationEvents.Ordering;

namespace Ordering.Worker.UnitTests.StateMachines.Transitions;

public class When_OrderInitiated_Tests : TestBase
{
    [Fact]
    public async Task Should_create_saga_and_schedule_messages_and_publish_events()
    {
        // Arrange
        var machine = new OrderStateMachine();
        var repository = new InMemorySagaRepository<OrderState>();

        var sagaHarness = Harness.StateMachineSaga(machine, repository);

        var orderId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        // Act
        await Harness.Bus.Publish(new OrderInitiated()
        {
            Id = orderId,
            Customer = "Test",
            Date = now
        });

        // Assert Saga Created
        var created = await sagaHarness.Created.Any(x => x.CorrelationId == orderId);
        Assert.True(created, "Saga instance was not created.");

        var instance = sagaHarness.Created.Contains(orderId);
        Assert.NotNull(instance);

        // Assert Saga State → WaitingForPayment
        Assert.Equal(machine.WaitingForPayment.Name, instance.CurrentState);

        // Assert two scheduled messages exist:
        // - SendReminder
        // - CancelOrder
        var scheduledReminder = Harness.Sent.Select<ScheduledMessage<SendReminder>>().Any();
        Assert.True(scheduledReminder, "SendReminder was not scheduled.");

        var scheduledCancel = Harness.Sent.Select<ScheduledMessage<CancelOrder>>().Any();
        Assert.True(scheduledCancel, "CancelOrder was not scheduled.");

        // Assert ReduceInventory published
        var reduceInv = Harness.Published.Select<ReduceInventory>().Any();
        Assert.True(reduceInv, "ReduceInventory was not published.");

        // Assert RemoveBasket published
        var removeBasket = Harness.Published.Select<RemoveBasket>().Any();
        Assert.True(removeBasket, "RemoveBasket was not published.");

        // Assert OrderStatusChanged published
        var statusChanged = Harness.Published
            .Select<OrderStatusChanged>()
            .Any(x =>
                x.Context.Message.Id == orderId &&
                x.Context.Message.OrderState == machine.WaitingForPayment.Name);

        Assert.True(statusChanged, "OrderStatusChanged was not published.");
    }
}