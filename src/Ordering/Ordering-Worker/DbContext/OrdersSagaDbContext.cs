using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using Ordering.Worker.Configurations.Saga;

namespace Ordering.Worker.DbContext;

public class OrdersSagaDbContext : SagaDbContext
{
    public OrdersSagaDbContext(DbContextOptions<OrdersSagaDbContext> options)
        : base(options)
    {
    }

    protected override IEnumerable<ISagaClassMap> Configurations
    {
        get { yield return new OrderStateMap(); }
    }
}