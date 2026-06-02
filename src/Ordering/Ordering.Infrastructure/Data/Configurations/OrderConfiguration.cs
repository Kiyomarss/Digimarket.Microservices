using Microsoft.EntityFrameworkCore;
using Ordering_Domain.Domain.Entities;
using Ordering_Domain.Domain.Enum;
using BuildingBlocks.EFCore.Configurations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ordering_Infrastructure.Data.Configurations;

public class OrderConfiguration : EntityTypeConfigurationBase<Order>
{
    public override void Configure(EntityTypeBuilder<Order> builder)
    {
        base.Configure(builder);

        ConfigureTable("orders");

        ConfigureId(x => x.Id);
        
        ConfigureDateTime(x => x.Date, isRequired: true);
        
        ConfigureTypeSafeEnum(
                              x => x.State, 
                              id => OrderState.FromId(id), 
                              state => state.Id,
                              columnName: "order_state_id"
                             );


        ConfigureGuid(x => x.UserId);

        ConfigureOneToManyCollection(
                                     x => x.Items,
                                     i => i.Order,
                                     i => i.OrderId,
                                     deleteBehavior: DeleteBehavior.Cascade);

        Ignore(x => x.TotalPrice);

        ConfigureIndex(x => x.Date);
        ConfigureIndex(x => new { x.UserId, x.State });
    }
}