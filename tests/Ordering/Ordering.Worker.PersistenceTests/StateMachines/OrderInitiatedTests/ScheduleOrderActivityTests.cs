using BuildingBlocks.IntegrationEvents;
using FluentAssertions;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ordering.TestingInfrastructure.Builders;
using Ordering.Worker.Configurations.Saga;
using Ordering.Worker.DbContext;
using Ordering.Worker.PersistenceTests.Fixtures;
using Ordering.Worker.PersistenceTests.TestBase.TestBase;
using Ordering.Worker.StateMachines;

namespace Ordering.Worker.PersistenceTests.StateMachines.OrderInitiatedTests;

public class ScheduleOrderActivityTests : WorkerAppTestBase
{
    public ScheduleOrderActivityTests(WorkerAppFactory fixture)
        : base(fixture) { }
    [Fact]
    public async Task ScheduleTokens_InDatabase()
    {
        await ResetDatabase();

        var orderId = Guid.NewGuid();

        await PublishAndAssertPublishedAsync(new OrderInitiatedBuilder().WithId(orderId).Build());

        var exists = await SagaHarness.Exists(orderId, x => x.WaitingForPayment);
        exists.Should().NotBeNull("Saga was not persisted");

        await using var dbScope = CreateDbContextScope();
        var db = dbScope.DbContext;
        
        var saga = await db.Set<OrderState>().FindAsync(orderId);
        saga.Should().NotBeNull();
        
        saga.ReminderScheduleTokenId.Should().NotBeEmpty("ScheduleOrderActivity stores reminder schedule token");
        saga.CancelScheduleTokenId.Should().NotBeEmpty("ScheduleOrderActivity stores cancel schedule token");
    }
}