using FluentAssertions;
using MassTransit.Testing;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Ordering.Worker.Configurations.Saga;
using Ordering.Worker.DbContext;
using Ordering.Worker.PersistenceTests.Fixtures;
using Ordering.Worker.StateMachines;

namespace Ordering.Worker.PersistenceTests.TestBase.TestBase;

[Collection("WorkerIntegration")]
public abstract class WorkerAppTestBase : IClassFixture<WorkerAppFactory>, IAsyncLifetime
{
    protected readonly WorkerAppFactory Fixture;
    protected readonly ITestHarness Harness;
    protected readonly OrdersSagaDbContext SagaDbContext;
    protected ISagaStateMachineTestHarness<OrderStateMachine, OrderState> SagaHarness { get; private set; } = default!;


    protected IServiceScope Scope { get; private set; } = default!;

    protected WorkerAppTestBase(WorkerAppFactory fixture)
    {
        Fixture = fixture;

        Scope = Fixture.Services.CreateScope();

        Harness = Scope.ServiceProvider.GetRequiredService<ITestHarness>();

        SagaHarness =
            Harness.GetSagaStateMachineHarness<OrderStateMachine, OrderState>();

        SagaDbContext =
            Scope.ServiceProvider.GetRequiredService<OrdersSagaDbContext>();
    }


    public async Task InitializeAsync()
    {
        await Harness.Start();
    }

    public async Task DisposeAsync()
    {
        await Harness.Stop();
        Scope.Dispose();
    }

    protected async Task ResetDatabase()
    {
        await Fixture.ResetDatabaseAsync();
    }
    
    protected async Task ReloadEntityAsync<TEntity>(TEntity entity) where TEntity : class
    {
        await SagaDbContext.Entry(entity).ReloadAsync();
    }
    
    protected async Task PublishAndAssertPublishedAsync<TEvent>(TEvent @event, int timeoutSeconds = 5)
        where TEvent : class
    {
        await Harness.Bus.Publish(@event);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
        var published = await Harness.Published.Any<TEvent>(cts.Token);

        published.Should().BeTrue($"{typeof(TEvent).Name} was not published");
    }

    protected async Task AssertPublishedAsync<TEvent>(int timeoutSeconds = 5)
        where TEvent : class
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));

        var published = await Harness.Published.Any<TEvent>(cts.Token);

        published.Should().BeTrue($"{typeof(TEvent).Name} was not published");
    }
    
    protected async Task AssertConsumedAsync<TEvent>(int timeoutSeconds = 5)
        where TEvent : class
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));

        var result = await Harness.Consumed
                                  .Any<TEvent>(cts.Token);

        result.Should().BeTrue($"{typeof(TEvent).Name} was not consumed");
    }
}