using Identity.Domain.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Data.Configurations
{
    public class UserRoleConfiguration : EntityTypeConfigurationBase<UserRole>
    {
        public override void Configure(EntityTypeBuilder<UserRole> builder)
        {
            base.Configure(builder);

            ConfigureTable("user_roles");

            ConfigurePrimaryKey(ur => new { ur.UserId, ur.RoleId });

            ConfigureOneToMany(
                               ur => ur.User,
                               u => u.UserRoles,
                               deleteBehavior: DeleteBehavior.Cascade
                              );

            ConfigureOneToMany(
                               ur => ur.Role,
                               r => r.UserRoles,
                               deleteBehavior: DeleteBehavior.Cascade
                              );
        }
    }
}