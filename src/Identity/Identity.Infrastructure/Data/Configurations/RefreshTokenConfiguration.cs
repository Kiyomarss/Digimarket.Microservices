using Identity.Domain.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Data.Configurations
{
    public class RefreshTokenConfiguration : EntityTypeConfigurationBase<RefreshToken>
    {
        public override void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            base.Configure(builder);

            ConfigureTable("refresh_tokens");

            ConfigurePrimaryKey(x => x.Id);

            ConfigureOneToMany(
                               x => x.User, 
                               u => u.RefreshTokens, 
                               deleteBehavior: DeleteBehavior.Cascade,
                               foreignKeyPropertyName: nameof(RefreshToken.UserId)
                              );

            ConfigureString(x => x.Token, maxLength: 500, isRequired: true);

            ConfigureDateTime(x => x.CreatedAt, isRequired: true);
            ConfigureDateTime(x => x.ExpiresAt, isRequired: true);

            ConfigureIndex(x => x.Token, isUnique: true);
            
            ConfigureIndex(x => x.UserId);
        }
    }
}