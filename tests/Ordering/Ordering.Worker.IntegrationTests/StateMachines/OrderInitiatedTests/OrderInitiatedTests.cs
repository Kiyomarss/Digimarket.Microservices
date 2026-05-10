using BuildingBlocks.IntegrationEvents.Basket;
using FluentAssertions;
using Ordering.TestingInfrastructure.Builders;
using Ordering.Worker.IntegrationTests.StateMachines.Fixtures;
using Ordering.Worker.IntegrationTests.StateMachines.TestBases;
using Ordering.Worker.StateMachines.Contracts.Events;

namespace Ordering.Worker.IntegrationTests.StateMachines.OrderInitiatedTests;

public class OrderInitiatedTests : OrderSagaTestBase
{
    public OrderInitiatedTests(OrderSagaFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task OrderInitiated_Should_Initialize_Order_And_Schedule_Jobs()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        var message = new OrderInitiatedBuilder()
                      .WithId(orderId)
                      .Build();

        // Act
        await PublishAndAssertPublishedAsync(message);

        // Assert state transition
        var saga = await SagaHarness.Exists(orderId, x => x.WaitingForPayment);
        saga.Should().NotBeNull();

        // Assert InitializeOrderActivity
        (await Harness.Published.Any<ReduceInventory>())
            .Should().BeTrue();

        (await Harness.Published.Any<RemoveBasket>())
            .Should().BeTrue();

        // Assert ScheduleOrderActivity

        (await Harness.Sent.Any<SendReminder>())
            .Should().BeTrue("SendReminder must be scheduled");

        (await Harness.Sent.Any<CancelOrder>())
            .Should().BeTrue("CancelOrder must be scheduled");

        // Assert saga tokens saved
        var instance = SagaHarness.Sagas.Contains(orderId);
        instance.Should().NotBeNull();

        instance.ReminderScheduleTokenId.Should().NotBeNull();
        instance.CancelScheduleTokenId.Should().NotBeNull();
    }
}