using Catalog_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog_Infrastructure.Data.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("product");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                   .IsRequired()
                   .HasColumnType("uuid")
                   .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(x => x.Name)
                   .IsRequired()
                   .HasMaxLength(200)
                   .HasColumnType("varchar(200)");

            builder.Property(x => x.Description)
                   .IsRequired(false)
                   .HasColumnType("text");

            builder.Property(x => x.Price)
                   .IsRequired()
                   .HasColumnType("bigint");

            builder.Property(x => x.CreatedAt)
                   .IsRequired()
                   .HasColumnType("timestamp without time zone")
                   .HasDefaultValueSql("NOW()");

            builder.Property(x => x.UpdatedAt)
                   .IsRequired(false)
                   .HasColumnType("timestamp without time zone");

            builder.Property(x => x.AttributesJson)
                   .IsRequired(false)
                   .HasColumnType("jsonb");
        }
    }
}