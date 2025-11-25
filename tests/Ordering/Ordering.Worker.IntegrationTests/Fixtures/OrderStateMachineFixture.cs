using System;
using System.Threading.Tasks;
using BuildingBlocks.UnitOfWork;
using DotNet.Testcontainers.Containers;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Ordering.Worker.Configurations.Saga;
using Ordering.Worker.DbContext;
using Ordering.Worker.Extensions;
using Shared.TestFixtures;

namespace Ordering.Worker.IntegrationTests.Fixtures;

public class OrderStateMachineFixture : IAsyncDisposable
{
    public IHost Host { get; }
    public IServiceProvider Services => Host.Services;

    private readonly IContainer _rabbitMqContainer;
    private readonly IContainer _postgresContainer;

    public OrderStateMachineFixture()
    {
        _rabbitMqContainer = TestContainerFactory.CreateRabbitMqContainer();
        _rabbitMqContainer.StartAsync().GetAwaiter().GetResult();

        _postgresContainer = TestContainerFactory.CreatePostgresContainer();
        _postgresContainer.StartAsync().GetAwaiter().GetResult();

        TestEnvironmentHelper.SetPostgresConnectionString(_postgresContainer);
        TestEnvironmentHelper.SetRabbitMqHost(_rabbitMqContainer);

        var builder = new HostApplicationBuilder();

        builder.Configuration.AddEnvironmentVariables();

        builder.Services.AddOrderingServices(builder.Configuration);

        Host = builder.Build();

        //DatabaseHelper.ApplyMigrations<OrdersSagaDbContext>(Services);
    }

    public async Task StartAsync()
    {
        await Host.StartAsync();

        var bus = Services.GetRequiredService<IBusControl>();
        await bus.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            var bus = Services.GetService<IBusControl>();
            if (bus != null)
                await bus.StopAsync();
        }
        catch { }

        await _rabbitMqContainer.DisposeAsync();
        await _postgresContainer.DisposeAsync();
        await Host.StopAsync();
    }
}