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
    protected readonly ISender Sender;
    protected readonly OrdersSagaDbContext SagaDbContext;
    protected readonly ITestHarness Harness;
    protected ISagaStateMachineTestHarness<OrderStateMachine, OrderState> SagaHarness { get; private set; } = default!;


    protected WorkerAppTestBase(WorkerAppFactory fixture)
    {
        Fixture = fixture;
        Sender = Fixture.Services.GetRequiredService<ISender>();
        SagaHarness = Fixture.Services.GetRequiredService<ISagaStateMachineTestHarness<OrderStateMachine, OrderState>>();
        SagaDbContext = Fixture.Services.GetRequiredService<OrdersSagaDbContext>();
        Harness = Fixture.Services.GetRequiredService<ITestHarness>();
    }

    public async Task InitializeAsync()
    {
        await Harness.Start();
    }

    public async Task DisposeAsync()
    {
        await Harness.Stop();
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
    
    protected async Task PublishAndAssertConsumedAsync<TEvent>(TEvent @event, int timeoutSeconds = 5)
        where TEvent : class
    {
        // Publish
        await Harness.Bus.Publish(@event);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));

        // Assert Published
        var published = await Harness.Published.Any<TEvent>(cts.Token);
        published.Should().BeTrue($"{typeof(TEvent).Name} was not published");

        // Assert Consumed
        var consumed = await Harness.Consumed.Any<TEvent>(cts.Token);
        consumed.Should().BeTrue($"{typeof(TEvent).Name} was not consumed");
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