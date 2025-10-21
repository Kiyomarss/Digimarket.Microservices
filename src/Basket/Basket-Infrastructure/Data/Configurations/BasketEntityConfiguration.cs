using Basket.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Basket.Infrastructure.Data.Configurations
{
    public class BasketEntityConfiguration : IEntityTypeConfiguration<BasketEntity>
    {
        public void Configure(EntityTypeBuilder<BasketEntity> builder)
        {
            builder.ToTable("baskets");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                   .IsRequired()
                   .HasColumnType("uuid");

            builder.Property(x => x.UserId)
                   .IsRequired()
                   .HasColumnType("uuid");

            builder.HasMany(x => x.Items)
                   .WithOne(i => i.Basket)
                   .HasForeignKey(i => i.BasketId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}