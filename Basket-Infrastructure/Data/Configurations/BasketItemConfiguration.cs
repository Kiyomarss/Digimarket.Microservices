using Basket.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Basket.Infrastructure.Data.Configurations
{
    public class BasketItemConfiguration : IEntityTypeConfiguration<BasketItem>
    {
        public void Configure(EntityTypeBuilder<BasketItem> builder)
        {
            builder.ToTable("basket_items");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                   .IsRequired()
                   .HasColumnType("uuid")
                   .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(x => x.CatalogId)
                   .IsRequired()
                   .HasColumnType("uuid");

            builder.Property(x => x.Quantity)
                   .IsRequired()
                   .HasColumnType("integer");

            builder.Property(x => x.BasketId)
                   .IsRequired()
                   .HasColumnType("uuid");

            builder.HasOne(x => x.Basket)
                   .WithMany(b => b.Items)
                   .HasForeignKey(x => x.BasketId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}