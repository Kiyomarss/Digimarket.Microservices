using MassTransit;
using MassTransit.Testing;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ordering_Infrastructure.Data.DbContext;
using Ordering.TestingInfrastructure.Fixtures;
using Xunit;

[Collection("ApiIntegration")]
public abstract class OrderingAppTestBase : IClassFixture<OrderingAppFactory>, IAsyncLifetime
{
    protected readonly OrderingAppFactory Fixture;
    protected readonly ISender Sender;
    protected readonly OrderingDbContext DbContext;
    protected readonly IBusControl Bus;

    protected OrderingAppTestBase()
    {
        Fixture = new OrderingAppFactory();
        Sender = Fixture.Services.GetRequiredService<ISender>();
        DbContext = Fixture.Services.GetRequiredService<OrderingDbContext>();
        Bus = Fixture.Services.GetRequiredService<IBusControl>();
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => Task.CompletedTask;

    protected async Task CleanupDatabase()
    {
        await DbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"orders\", \"order_items\" RESTART IDENTITY CASCADE");
        await DbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"OutboxMessage\", \"OutboxState\", \"InboxState\" RESTART IDENTITY CASCADE");
    }
}