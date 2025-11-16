using BuildingBlocks.UnitOfWork;

namespace Ordering.Worker.IntegrationTests.TestFixtures
{
    public class MockUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
    }
}