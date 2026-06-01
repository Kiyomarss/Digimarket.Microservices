using Basketing.Domain.Entities;
using BuildingBlocks.EFCore.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Basketing.Infrastructure.Data.Configurations
{
    public class BasketEntityConfiguration : EntityTypeConfigurationBase<BasketEntity>
    {
        public override void Configure(EntityTypeBuilder<BasketEntity> builder)
        {
            base.Configure(builder);

            ConfigureTable("baskets");
            ConfigurePrimaryKey(x => x.Id);
            
            ConfigureGuid(x => x.UserId);
            
            ConfigureOneToManyCollection(x => x.Items, ur => ur.Basket, ur => ur.BasketEntityId, DeleteBehavior.Cascade);
        }
    }
}