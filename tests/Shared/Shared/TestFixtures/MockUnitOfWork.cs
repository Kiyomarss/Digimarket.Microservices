using BuildingBlocks.UnitOfWork;

namespace Shared.TestFixtures
{
    public class MockUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
    }
}