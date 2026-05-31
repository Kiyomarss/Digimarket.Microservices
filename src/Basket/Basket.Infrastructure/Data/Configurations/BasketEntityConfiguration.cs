using Basket.Domain.Entities;
using BuildingBlocks.EFCore.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Basket.Infrastructure.Data.Configurations
{
    public class BasketEntityConfiguration : EntityTypeConfigurationBase<BasketEntity>
    {
        public override void Configure(EntityTypeBuilder<BasketEntity> builder)
        {
            base.Configure(builder);

            ConfigureTable("baskets");
            ConfigurePrimaryKey(x => x.Id);
            
            ConfigureGuid(x => x.UserId);
            
            ConfigureOneToManyCollection(
                                         collectionExpression: x => x.Items,
                                         inverseNavigationExpression: i => i.Basket,
                                         foreignKeyExpression: i => i.BasketEntityId,
                                         deleteBehavior: DeleteBehavior.Cascade
                                        );
        }
    }
}