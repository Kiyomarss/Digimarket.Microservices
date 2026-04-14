using FluentAssertions;
using MassTransit.Testing;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Ordering_Infrastructure.Data.DbContext;
using Ordering.TestingInfrastructure.Fixtures;
using Xunit;

namespace Ordering.TestingInfrastructure.TestBase;

[Collection("ApiIntegration")]
public abstract class OrderingAppTestBase : IClassFixture<OrderingAppFactory>, IAsyncLifetime
{
    protected readonly OrderingAppFactory Fixture;
    protected readonly ISender Sender;
    protected readonly OrderingDbContext DbContext;
    protected readonly ITestHarness Harness;

    protected OrderingAppTestBase(OrderingAppFactory fixture)
    {
        Fixture = fixture;
        Sender = Fixture.Services.GetRequiredService<ISender>();
        DbContext = Fixture.Services.GetRequiredService<OrderingDbContext>();
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
        await DbContext.Entry(entity).ReloadAsync();
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