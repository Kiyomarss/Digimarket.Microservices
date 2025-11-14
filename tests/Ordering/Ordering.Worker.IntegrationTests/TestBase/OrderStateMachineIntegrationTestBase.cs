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
    public abstract class OrderStateMachineIntegrationTestBase : IAsyncLifetime
    {
        protected readonly IBusControl Bus;
        protected readonly OrdersSagaDbContext DbContext;

        private readonly OrderStateMachineFixture _fixture;

        protected OrderStateMachineIntegrationTestBase()
        {
            _fixture = new OrderStateMachineFixture();
            Bus = _fixture.Bus;
            DbContext = _fixture.DbContext;
        }

        public async Task InitializeAsync()
        {
            await _fixture.StartAsync();
        }

        public async Task DisposeAsync()
        {
            await _fixture.DisposeAsync();
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