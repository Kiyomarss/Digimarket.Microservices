using BuildingBlocks.EFCore.Configurations;
using Identity.Domain.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Data.Configurations
{
    public class UserClaimConfiguration : EntityTypeConfigurationBase<UserClaim>
    {
        public override void Configure(EntityTypeBuilder<UserClaim> builder)
        {
            base.Configure(builder);

            ConfigureTable("user_claims");

            ConfigurePrimaryKey(x => new { x.UserId, x.Type, x.Value });

            ConfigureOneToMany(
                               uc => uc.User, 
                               u => u.UserClaims, 
                               deleteBehavior: DeleteBehavior.Cascade
                              );

            ConfigureString(x => x.Type, maxLength: 255, isRequired: true);
            ConfigureString(x => x.Value, maxLength: 255, isRequired: true);
        }
    }
}