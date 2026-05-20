using Identity.Domain.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Data.Configurations
{
    public class UserClaimConfiguration : IEntityTypeConfiguration<UserClaim>
    {
        public void Configure(EntityTypeBuilder<UserClaim> builder)
        {
            builder.ToTable("user_claims");

            builder.HasKey(x => new
            {
                x.UserId,
                x.Type,
                x.Value
            });
            
            builder.HasOne(uc => uc.User)
                   .WithMany(u => u.Claims)
                   .HasForeignKey(uc => uc.UserId)
                   .Metadata.PrincipalToDependent?.SetField("_claims");
            
            builder.HasIndex(x => x.UserId);

            builder.Property(x => x.Type).IsRequired().HasMaxLength(255);

            builder.Property(x => x.Value).IsRequired().HasMaxLength(255);
        }
    }
}