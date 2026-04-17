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

public class InitializeOrderActivityTests : WorkerAppTestBase
{
    public InitializeOrderActivityTests(WorkerAppFactory fixture)
        : base(fixture) { }
    [Fact]
    public async Task InitializeOrderActivity_ShouldPersistDate_InDatabase()
    {
        await ResetDatabase();

        var orderId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        await PublishAndAssertPublishedAsync(
                                             new OrderInitiatedBuilder()
                                                 .WithId(orderId)
                                                 .WithDate(now)
                                                 .Build());
        var exists = await SagaHarness.Exists(orderId, x => x.WaitingForPayment);
        exists.Should().NotBeNull("Saga was not persisted");

        using var scope = Fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrdersSagaDbContext>();

        var saga = await db.Set<OrderState>().FindAsync(orderId);
        saga.Should().NotBeNull();
        
        saga!.Date.Should().BeCloseTo(now, TimeSpan.FromSeconds(1));
    }
}