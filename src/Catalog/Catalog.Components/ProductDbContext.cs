using System.Text.Json;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Components;

public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options)
        : base(options) { }
    
    public DbSet<Product> Catalogs { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }
    public DbSet<Category> Categores { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        MapRegistration(modelBuilder);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }

    private static void MapRegistration(ModelBuilder modelBuilder)
    {
        // Product entity
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");

            entity.HasKey(p => p.Id);

            entity.Property(p => p.Name)
                  .HasMaxLength(64)
                  .IsRequired();

            entity.Property(p => p.Description)
                  .HasMaxLength(256)
                  .IsRequired();

            entity.Property(p => p.Price)
                  .IsRequired();

            entity.Property(p => p.StockQuantity)
                  .IsRequired();
            
            entity.Property(p => p.ReservedQuantity)
                  .IsRequired();

            entity.HasOne(p => p.Category)
                  .WithMany()
                  .HasForeignKey(c => c.CategoryId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(p => p.Images)
                  .WithOne(img => img.Product)
                  .HasForeignKey(img => img.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");

            entity.HasKey(c => c.Id);

            entity.Property(c => c.Name)
                  .HasMaxLength(200)
                  .IsRequired();

            entity.HasOne(c => c.Parent)
                  .WithMany()
                  .HasForeignKey(c => c.ParentId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.ToTable("ProductImages");

            entity.HasKey(pi => pi.Id);

            entity.Property(pi => pi.Url)
                  .HasMaxLength(200)
                  .IsRequired();

            entity.Property(pi => pi.AltText)
                  .HasMaxLength(200);
        });
    }
    
}