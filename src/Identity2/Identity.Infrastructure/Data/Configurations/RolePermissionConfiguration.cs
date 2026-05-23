using Identity.Domain.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Data.Configurations
{
    public class RolePermissionConfiguration : EntityTypeConfigurationBase<RolePermission>
    {
        public override void Configure(EntityTypeBuilder<RolePermission> builder)
        {
            base.Configure(builder);

            ConfigureTable("role_permissions");

            ConfigurePrimaryKey(x => new { x.RoleId, x.PermissionId });

            ConfigureOneToMany(
                               x => x.Role,
                               r => r.RolePermissions
                              );

            ConfigureOneToMany(x => x.Permission);
        }
    }
}