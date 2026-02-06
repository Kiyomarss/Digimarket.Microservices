using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordering_Domain.Domain.Entities;
using Ordering_Domain.Domain.Enum;

namespace Ordering_Infrastructure.Data.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
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
                   .HasColumnName("order_state_id")
                   .HasColumnType("integer")
                   .HasConversion(state => state.Id, id => OrderState.FromId(id));
            
            builder.Property(x => x.UserId)
                   .IsRequired()
                   .HasColumnType("uuid");

            // ارتباط یک‌به‌چند بین Order و OrderItem
            builder.HasMany(x => x.Items)
                   .WithOne(i => i.Order)
                   .HasForeignKey(i => i.OrderId)
                   .OnDelete(DeleteBehavior.Cascade);

            // پراپرتی محاسباتی ذخیره نمی‌شود
            builder.Ignore(x => x.TotalPrice);
            
            builder.HasIndex(x => x.Date);
            
            builder.HasIndex(x => new { x.UserId, x.State });
        }
    }
}