using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ordering.Worker.Configurations.Saga;

public class OrderStateMap :
    SagaClassMap<OrderState>
{
    protected override void Configure(EntityTypeBuilder<OrderState> entity, ModelBuilder model)
    {
        entity.Property(x => x.CurrentState);
        
        entity.Property(x => x.Date);
    }
}

/*این کلاس دقیقاً معادل IEntityTypeConfiguration<T> در EF Core است (Fluent API).
 MassTransit از طریق کلاس SagaClassMap<T> به شما اجازه می‌دهد
نحوه تبدیل شدن کلاس OrderState به جدول دیتابیس را مدیریت کنید.*/