using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ordering.Components.StateMachines;

public class OrderStateMap :
    SagaClassMap<OrdersState>
{
    protected override void Configure(EntityTypeBuilder<OrdersState> entity, ModelBuilder model)
    {
        entity.Property(x => x.CurrentState);
        
        entity.Property(x => x.Date);
        entity.Property(x => x.Customer);
    }
}