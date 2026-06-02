using Basketing.Domain.Entities;
using BuildingBlocks.EFCore.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Basketing.Infrastructure.Data.Configurations
{
    public class BasketItemConfiguration : EntityTypeConfigurationBase<BasketItem>
    {
        public override void Configure(EntityTypeBuilder<BasketItem> builder)
        {
            base.Configure(builder);

            ConfigureTable("basket_items");
            
            ConfigureId(x => x.Id);

            ConfigureGuid(x => x.ProductId);
            
            ConfigureInteger(x => x.Quantity, isRequired: true);
            
            ConfigureOneToMany(
                               uc => uc.Basket, 
                               u => u.Items, 
                               deleteBehavior: DeleteBehavior.Cascade
                              );
        }
    }
}