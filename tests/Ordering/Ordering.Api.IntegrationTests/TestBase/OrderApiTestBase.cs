using MassTransit;
using MassTransit.Testing;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ordering_Infrastructure.Data.DbContext;
using Ordering.Api.IntegrationTests.Fixtures;

namespace Ordering.Api.IntegrationTests.TestBase;

[Collection("Integration")]
public abstract class OrderApiTestBase : IClassFixture<OrderingApiFactory>, IAsyncLifetime
{
    protected readonly OrderingApiFactory Fixture;
    protected readonly ISender Sender;
    protected readonly OrderingDbContext DbContext;
    protected readonly ITestHarness TestHarness;
    protected readonly IBusControl Bus;

    protected OrderApiTestBase()
    {
        Fixture = new OrderingApiFactory();
        Bus = Fixture.Bus;
        Sender = Fixture.Services.GetRequiredService<ISender>();
        DbContext = Fixture.Services.GetRequiredService<OrderingDbContext>();
        TestHarness = Fixture.Services.GetRequiredService<ITestHarness>();
    }

    public async Task InitializeAsync()
    {
        await Fixture.StartAsync();
    }    
    
    public async Task DisposeAsync()
    {
        await Fixture.DisposeAsync();
    }
    protected async Task CleanupDatabase()
    {
        await DbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"orders\", \"order_items\" RESTART IDENTITY CASCADE");
        await DbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"OutboxMessage\", \"OutboxState\", \"InboxState\" RESTART IDENTITY CASCADE");
    }
}