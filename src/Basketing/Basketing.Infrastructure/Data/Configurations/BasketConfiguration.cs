using Basketing.Domain.Entities;
using BuildingBlocks.EFCore.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Basketing.Infrastructure.Data.Configurations
{
    public class BasketConfiguration : EntityTypeConfigurationBase<Basket>
    {
        public override void Configure(EntityTypeBuilder<Basket> builder)
        {
            base.Configure(builder);

            ConfigureTable("baskets");
            
            ConfigureId(x => x.Id);
            
            ConfigureGuid(x => x.UserId);
            
            ConfigureOneToManyCollection(x => x.Items, ur => ur.Basket, ur => ur.BasketId, DeleteBehavior.Cascade);
        }
    }
}