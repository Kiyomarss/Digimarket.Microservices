using BuildingBlocks.IntegrationEvents;
using FluentAssertions;
using MassTransit.Testing;
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
    public async Task InitializeOrderActivity_ShouldPublishRequiredEvents_AndTransitionToWaitingForPayment()
    {
        var orderId = Guid.NewGuid();

        await PublishAndAssertPublishedAsync(new OrderInitiatedBuilder().WithId(orderId).Build());

        var exists = await SagaHarness.Exists(orderId, x => x.WaitingForPayment);
        exists.Should().NotBeNull("saga must be created and transitioned to WaitingForPayment");

        // Assert: Two events must be published by InitializeOrderActivity
        (await Harness.Published.Any<ReduceInventory>()).Should().BeTrue("ReduceInventory must be published");
        (await Harness.Published.Any<RemoveBasket>()).Should().BeTrue("RemoveBasket must be published");
    }
}