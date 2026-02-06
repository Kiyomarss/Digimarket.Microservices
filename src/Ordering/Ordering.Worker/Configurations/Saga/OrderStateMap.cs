using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ordering.Worker.Configurations.Saga;

//TODO: کاربرد کد زیر چیست؟ چرا دیگر پراپرتی ها به کد زیر اضافه نشده؟
public class OrderStateMap :
    SagaClassMap<OrderState>
{
    protected override void Configure(EntityTypeBuilder<OrderState> entity, ModelBuilder model)
    {
        entity.Property(x => x.CurrentState);
        
        entity.Property(x => x.Date);
    }
}