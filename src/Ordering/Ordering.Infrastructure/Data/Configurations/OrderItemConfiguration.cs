using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordering_Domain.Domain.Entities;
using BuildingBlocks.EFCore.Configurations;

namespace Ordering_Infrastructure.Data.Configurations;

public class OrderItemConfiguration : EntityTypeConfigurationBase<OrderItem>
{
    public override void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        base.Configure(builder);

        ConfigureTable("order_items");
        
        ConfigureId(x => x.Id);

        ConfigureGuid(x => x.OrderId);
        ConfigureGuid(x => x.ProductId);

        ConfigureInteger(x => x.Quantity);
        
        ConfigureBigInt(x => x.Price);
    }
}