using MassTransit;
using MassTransit.RetryPolicies;

namespace Ordering.Worker;

public class RecreateDatabaseHostedService<TDbContext> : IHostedService
    where TDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    private readonly ILogger<RecreateDatabaseHostedService<TDbContext>> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public RecreateDatabaseHostedService(IServiceScopeFactory scopeFactory, ILogger<RecreateDatabaseHostedService<TDbContext>> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Ensuring database for {DbContext}", TypeCache<TDbContext>.ShortName);

        await Retry.Interval(20, TimeSpan.FromSeconds(3)).Retry(async () =>
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<TDbContext>();

            // فقط ایجاد در صورت نیاز، حذف نمی‌کنیم
            await context.Database.EnsureCreatedAsync(cancellationToken);

            _logger.LogInformation("Database ensured for {DbContext}", TypeCache<TDbContext>.ShortName);
        }, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}