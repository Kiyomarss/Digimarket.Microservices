using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordering.Components.Domain.Entities;

namespace Ordering_Infrastructure.Data.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Ordering.Components.Domain.Entities.Order>
    {
        public void Configure(EntityTypeBuilder<Ordering.Components.Domain.Entities.Order> builder)
        {
            builder.ToTable("orders");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                   .IsRequired()
                   .HasColumnType("uuid")
                   .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(x => x.Date)
                   .IsRequired()
                   .HasColumnType("timestamp without time zone")
                   .HasDefaultValueSql("NOW()");

            builder.Property(x => x.State)
                   .IsRequired()
                   .HasMaxLength(100)
                   .HasColumnType("varchar(100)");

            builder.Property(x => x.Customer)
                   .IsRequired()
                   .HasMaxLength(200)
                   .HasColumnType("varchar(200)");

            // ارتباط یک‌به‌چند بین Order و OrderItem
            builder.HasMany(x => x.Items)
                   .WithOne(i => i.Order)
                   .HasForeignKey(i => i.OrderId)
                   .OnDelete(DeleteBehavior.Cascade);

            // پراپرتی محاسباتی ذخیره نمی‌شود
            builder.Ignore(x => x.TotalPrice);
        }
    }
}