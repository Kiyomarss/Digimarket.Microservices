using BuildingBlocks.Common.Entities;
using BuildingBlocks.Common.Events;
using MediatR;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BuildingBlocks.Common.Interceptors;

public sealed class DomainEventsInterceptor : SaveChangesInterceptor
{
    private readonly IMediator _mediator;

    public DomainEventsInterceptor(IMediator mediator) => _mediator = mediator;

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context == null) return result;

        // 1. جمع‌آوری تمام دامین ایونت‌ها
        var domainEvents = context.ChangeTracker
                                  .Entries<Entity>()
                                  .Where(e => e.Entity.DomainEvents.Any())
                                  .SelectMany(e => e.Entity.DomainEvents)
                                  .ToList();

        // 2. پاک کردن دامین ایونت‌ها از انتیتی‌ها (برای جلوگیری از تکرار)
        foreach (var entry in context.ChangeTracker.Entries<Entity>())
        {
            entry.Entity.ClearDomainEvents();
        }

        // 3. انتشار دامین ایونت‌ها (قبل از ذخیره‌سازی)
        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
        }

        // 4. اجازه دادن به EF Core که SaveChanges را ادامه دهد
        return result; // این خط مهم است — به EF می‌گوید ادامه بده
    }

    // اگر می‌خواهی دقیق‌تر باشی، می‌توانی base را هم صدا بزنی
    // اما معمولاً return result کافی است
}