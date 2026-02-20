using FluentAssertions;
using MassTransit.Testing;
using Ordering.Worker.IntegrationTests.StateMachines.Fixtures;
using Ordering.Worker.IntegrationTests.StateMachines.TestBases;
using Ordering.Worker.StateMachines.Events;
using Shared.IntegrationEvents.Ordering;

namespace Ordering.Worker.IntegrationTests.StateMachines.OrderInitiatedTests;

public class OrderInitiatedTests : OrderSagaTestBase
{
    public OrderInitiatedTests(OrderSagaFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task Should_create_saga_and_transition_to_waiting_for_payment()
    {
        var orderId = Guid.NewGuid();
        var date = DateTime.UtcNow;

        await Harness.Bus.Publish(new OrderInitiated
        {
            Id = orderId,
            Date = date
        });

        (await SagaHarness.Consumed.Any<OrderInitiated>())
            .Should().BeTrue();

        var instance = SagaHarness.Created.ContainsInState(
                                                           orderId,
                                                           Machine,
                                                           Machine.WaitingForPayment);

        instance.Should().NotBeNull();
        instance!.Date.Should().Be(date);
    }
    
    [Fact]
    public async Task Should_publish_reduce_inventory_and_remove_basket()
    {
        var orderId = Guid.NewGuid();

        await Harness.Bus.Publish(new OrderInitiated
        {
            Id = orderId,
            Date = DateTime.UtcNow
        });

        (await Harness.Published.Any<ReduceInventory>())
            .Should().BeTrue();

        (await Harness.Published.Any<RemoveBasket>())
            .Should().BeTrue();
    }
    
    [Fact]
    public async Task Should_schedule_reminder_and_cancel_messages()
    {
        var orderId = Guid.NewGuid();

        await Harness.Bus.Publish(new OrderInitiated
        {
            Id = orderId,
            Date = DateTime.UtcNow
        });

        await Harness.InactivityTask;

        var instance = SagaHarness.Created.Contains(orderId);

        instance.Should().NotBeNull();
        instance.ReminderScheduleTokenId.Should().NotBeNull();
        instance.CancelScheduleTokenId.Should().NotBeNull();
    }
}