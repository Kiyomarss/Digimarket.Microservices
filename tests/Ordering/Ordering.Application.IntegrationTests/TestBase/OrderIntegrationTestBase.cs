// tests/Ordering.Application.IntegrationTests/TestBase/OrderIntegrationTestBase.cs
using FluentAssertions;
using MassTransit.Testing;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ordering_Infrastructure.Data.DbContext;
using Ordering.Application.IntegrationTests.Fixtures;
using Xunit;

namespace Ordering.Application.IntegrationTests.TestBase;

[Collection("Integration")]
public abstract class OrderIntegrationTestBase : IClassFixture<OrderingIntegrationFixture>, IAsyncLifetime
{
    protected readonly OrderingIntegrationFixture Fixture;
    protected readonly ISender Sender;
    protected readonly OrderingDbContext DbContext;
    protected readonly ITestHarness TestHarness;

    protected OrderIntegrationTestBase()
    {
        Fixture = new OrderingIntegrationFixture();
        Sender = Fixture.Services.GetRequiredService<ISender>();
        DbContext = Fixture.Services.GetRequiredService<OrderingDbContext>();
        TestHarness = Fixture.Services.GetRequiredService<ITestHarness>();
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => Task.CompletedTask;

    protected async Task CleanupDatabase()
    {
        await DbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"orders\", \"order_items\" RESTART IDENTITY CASCADE");
        await DbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"OutboxMessage\", \"OutboxState\", \"InboxState\" RESTART IDENTITY CASCADE");
    }
}