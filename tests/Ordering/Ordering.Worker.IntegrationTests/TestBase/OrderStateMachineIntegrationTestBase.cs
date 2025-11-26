using System.Threading.Tasks;
using FluentAssertions;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ordering.Worker.Configurations.Saga;
using Ordering.Worker.DbContext;
using Ordering.Worker.IntegrationTests.Fixtures;
using Xunit;

namespace Ordering.Worker.IntegrationTests.TestBase
{
    public abstract class OrderStateMachineIntegrationTestBase : IAsyncLifetime
    {
        protected readonly OrdersSagaDbContext DbContext;
        protected readonly IBusControl Bus;
        protected readonly IServiceScope Scope;
        
        //NOTE: در صورت استفاده نشدن از ورودی کانستراکتور و new کردن آبجکت کانستراکتور دوبار اجرا و هر کانتینر دوبار ساخته می‌شود
        public OrderStateMachineIntegrationTestBase(OrderStateMachineFixture fixture)
        {
            fixture.StartAsync().GetAwaiter().GetResult();
            Scope = fixture.Services.CreateScope();

            DbContext = Scope.ServiceProvider.GetRequiredService<OrdersSagaDbContext>();
            Bus = Scope.ServiceProvider.GetRequiredService<IBusControl>();
        }

        public Task InitializeAsync() => Task.CompletedTask;
        public Task DisposeAsync() => Task.CompletedTask;

        // متد کمکی برای پاک کردن دیتابیس
        protected async Task CleanupDatabase()
        {
            await DbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"OrderState\"");
            await DbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"OutboxMessage\"");
            await DbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"OutboxState\"");
            await DbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"InboxState\"");
        }

        // متد کمکی برای دریافت Saga
        protected async Task<OrderState?> GetSagaState(Guid orderId)
        {
            return await DbContext.Set<OrderState>()
                                  .FirstOrDefaultAsync(s => s.CorrelationId == orderId);
        }
    }
}