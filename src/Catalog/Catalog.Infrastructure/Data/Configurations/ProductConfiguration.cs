using BuildingBlocks.EFCore.Configurations;
using Catalog_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog_Infrastructure.Data.Configurations
{
    public class ProductConfiguration : EntityTypeConfigurationBase<Product>
    {
        public override void Configure(EntityTypeBuilder<Product> builder)
        {
            base.Configure(builder);

            ConfigureTable("products");
            
            ConfigureId(x => x.Id);
            
            ConfigureString(x => x.Name, maxLength: 200, isRequired: true);

            ConfigureText(x => x.Description, isRequired: true);
            
            ConfigureInteger(x => x.Stock);
            ConfigureBigInt(x => x.Price);
            
            ConfigureDateTime(x => x.CreatedAt, isRequired: true);
            ConfigureDateTime(x => x.UpdatedAt);

            ConfigureJsonb(x => x.Attributes);
        }
    }
}