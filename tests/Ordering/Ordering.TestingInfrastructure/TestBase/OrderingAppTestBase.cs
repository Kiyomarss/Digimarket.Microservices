using FluentAssertions;
using MassTransit;
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
        Fixture.Services.GetRequiredService<IBusControl>();// بررسی جهت حذف
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => Task.CompletedTask;

    protected async Task ResetDatabase()
    {
        await Fixture.ResetDatabaseAsync();
    }
    
    protected async Task ReloadEntityAsync<TEntity>(TEntity entity) where TEntity : class
    {
        await DbContext.Entry(entity).ReloadAsync();
    }
    
    protected Task PublishEventAsync<TEvent>(TEvent @event) where TEvent : class
    {
        return Harness.Bus.Publish(@event);
    }

    protected async Task AssertPublishedAsync<TEvent>(int timeoutSeconds = 5)
        where TEvent : class
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));

        var published = await Harness.Published.Any<TEvent>(cts.Token);

        published.Should().BeTrue($"{typeof(TEvent).Name} was not published");
    }
}