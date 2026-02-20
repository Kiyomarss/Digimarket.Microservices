using MassTransit;
using MassTransit.Testing;
using Ordering.Worker.Configurations.Saga;
using Ordering.Worker.DbContext;
using Ordering.Worker.PersistenceTests.Fixtures;
using Ordering.Worker.StateMachines;

namespace Ordering.Worker.PersistenceTests.TestBase;

public abstract class WorkerPersistenceTestBase : 
    IClassFixture<OrderingWorkerPersistenceFixture>, 
    IAsyncLifetime
{
    protected readonly OrderingWorkerPersistenceFixture Fixture;

    protected ISagaStateMachineTestHarness<OrderStateMachine, OrderState> SagaHarness => Fixture.SagaHarness;
    protected IBusControl Bus => Fixture.Bus;
    protected OrdersSagaDbContext DbContext => Fixture.DbContext;

    protected WorkerPersistenceTestBase(OrderingWorkerPersistenceFixture fixture)
    {
        Fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask; // Fixture خودش InitializeAsync دارد
    public Task DisposeAsync() => Task.CompletedTask;

    protected async Task ResetDatabaseAsync() => await Fixture.ResetDatabaseAsync();

    protected async Task ReloadEntityAsync<TEntity>(TEntity entity) where TEntity : class
        => await DbContext.Entry(entity).ReloadAsync();

    protected async Task PublishEventAsync<TEvent>(TEvent @event) where TEvent : class
        => await Bus.Publish(@event);

    protected async Task AssertPublishedAsync<TEvent>(int timeoutSeconds = 5) where TEvent : class
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
        var harness = (InMemoryTestHarness)SagaHarness;
        var published = await harness.Published.Any<TEvent>(cts.Token);
        Assert.True(published, $"{typeof(TEvent).Name} was not published");
    }
}