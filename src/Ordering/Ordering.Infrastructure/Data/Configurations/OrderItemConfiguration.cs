using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordering_Domain.Domain.Entities;

namespace Ordering_Infrastructure.Data.Configurations
{
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("order_items");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                   .IsRequired()
                   .HasColumnType("uuid")
                   .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(x => x.OrderId)
                   .IsRequired()
                   .HasColumnType("uuid");

            builder.Property(x => x.ProductId)
                   .IsRequired()
                   .HasColumnType("uuid");

            builder.Property(x => x.ProductName)
                   .IsRequired()
                   .HasMaxLength(200)
                   .HasColumnType("varchar(200)");

            builder.Property(x => x.Quantity)
                   .IsRequired()
                   .HasColumnType("integer");

            builder.Property(x => x.Price)
                   .IsRequired()
                   .HasColumnType("bigint");
        }
    }
}