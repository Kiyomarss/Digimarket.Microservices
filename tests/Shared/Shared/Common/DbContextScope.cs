using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Common;

public sealed class DbContextScope<T> : IAsyncDisposable
    where T : DbContext
{
    public IServiceScope Scope { get; }
    public T DbContext { get; }

    public DbContextScope(IServiceScope scope, T db)
    {
        Scope = scope;
        DbContext = db;
    }

    public async ValueTask DisposeAsync()
    {
        await DbContext.DisposeAsync();
        Scope.Dispose();
    }
}
