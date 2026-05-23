using Identity.Domain.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Data.Configurations;

public class UserConfiguration : EntityTypeConfigurationBase<User>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        base.Configure(builder);

        ConfigureTable("users");
        ConfigurePrimaryKey(x => x.Id);
        
        Builder.Property(x => x.Id).ValueGeneratedNever();

        ConfigureString(x => x.Email, isRequired: true, maxLength: 256);
        ConfigureIndex(x => x.Email, isUnique: true);
        
        ConfigureString(x => x.PasswordHash, isRequired: true);
        ConfigureDateTime(x => x.CreatedAt, isRequired: true);

        Ignore(x => x.Roles);

        ConfigureOneToManyCollection(x => x.UserRoles, ur => ur.User, ur => ur.UserId, DeleteBehavior.Cascade);
        ConfigureOneToManyCollection(x => x.UserClaims, uc => uc.User, uc => uc.UserId, DeleteBehavior.Cascade);
        ConfigureOneToManyCollection(x => x.RefreshTokens, rt => rt.User, rt => rt.UserId, DeleteBehavior.Cascade);
    }
}