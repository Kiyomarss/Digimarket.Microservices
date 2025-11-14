using System.Threading.Tasks;
using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Ordering.Worker.Configurations.Saga;
using Ordering.Worker.DbContext;
using Ordering.Worker.IntegrationTests.Fixtures;
using Xunit;

namespace Ordering.Worker.IntegrationTests.TestBase
{
    public abstract class OrderStateMachineIntegrationTestBase : IClassFixture<OrderStateMachineFixture>
    {
        protected readonly IBusControl Bus;
        protected readonly OrdersSagaDbContext DbContext;

        protected OrderStateMachineIntegrationTestBase(OrderStateMachineFixture fixture)
        {
            Bus = fixture.Bus;
            DbContext = fixture.DbContext;

            // شروع منابع در Fixture
            fixture.StartAsync().GetAwaiter().GetResult();
        }

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