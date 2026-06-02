using BuildingBlocks.EFCore.Configurations;
using Identity.Domain.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Data.Configurations
{
    public class PermissionConfiguration : EntityTypeConfigurationBase<Permission>
    {
        public override void Configure(EntityTypeBuilder<Permission> builder)
        {
            base.Configure(builder);

            ConfigureTable("permissions");
            
            ConfigureId(x => x.Id);

            ConfigureString(x => x.Name, maxLength: 200, isRequired: true);

            ConfigureIndex(x => x.Name, isUnique: true);
        }
    }
}