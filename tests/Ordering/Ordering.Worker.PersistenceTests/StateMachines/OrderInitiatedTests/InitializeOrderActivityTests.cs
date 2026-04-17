using BuildingBlocks.IntegrationEvents;
using FluentAssertions;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Ordering.TestingInfrastructure.Builders;
using Ordering.Worker.Configurations.Saga;
using Ordering.Worker.DbContext;
using Ordering.Worker.PersistenceTests.Fixtures;
using Ordering.Worker.PersistenceTests.TestBase.TestBase;

namespace Ordering.Worker.PersistenceTests.StateMachines.OrderInitiatedTests;

public class InitializeOrderActivityTests : WorkerAppTestBase
{
    public InitializeOrderActivityTests(WorkerAppFactory fixture)
        : base(fixture) { }
    [Fact]
    public async Task Should_create_saga_and_publish_reduce_inventory_and_remove_basket()
    {
        await ResetDatabase();

        var orderId = Guid.NewGuid();

        await PublishAndAssertConsumedAsync(new OrderInitiatedBuilder().WithId(orderId).Build());
        
        var instance = SagaHarness.Created.ContainsInState(orderId, SagaHarness.StateMachine, SagaHarness.StateMachine.WaitingForPayment);

        instance.Should().NotBeNull();
    }
}