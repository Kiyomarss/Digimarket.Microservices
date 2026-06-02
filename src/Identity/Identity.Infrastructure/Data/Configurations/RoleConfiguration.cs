using BuildingBlocks.EFCore.Configurations;
using Identity.Domain.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Data.Configurations
{
    public class RoleConfiguration : EntityTypeConfigurationBase<Role>
    {
        public override void Configure(EntityTypeBuilder<Role> builder)
        {
            base.Configure(builder);

            ConfigureTable("roles");
            
            ConfigureId(x => x.Id);

            ConfigureString(x => x.Name, maxLength: 100, isRequired: true);

            ConfigureIndex(x => x.Name, isUnique: true);
        }
    }
}