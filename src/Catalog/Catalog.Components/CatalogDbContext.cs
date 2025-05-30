using System.Text.Json;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Components;

public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
        : base(options) { }
    
    public DbSet<Catalog> Catalogs { get; set; }
    public DbSet<CatalogItem> CatalogItems { get; set; }
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
        modelBuilder.Entity<Catalog>(entity =>
        {
            entity.ToTable("Catalogs");
            entity.HasKey(o => o.Id);

            entity.Property(o => o.Date).IsRequired();
            entity.Property(o => o.Customer)
                  .HasMaxLength(64)
                  .IsRequired();

            entity.HasMany(o => o.Items)
                  .WithOne(oi => oi.Catalog)
                  .HasForeignKey(oi => oi.CatalogId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CatalogItem>(entity =>
        {
            entity.ToTable("Catalog_items");
            entity.HasKey(oi => oi.Id);

            entity.Property(oi => oi.ProductId).IsRequired();
            entity.Property(oi => oi.ProductName)
                  .HasMaxLength(200)
                  .IsRequired();
            entity.Property(oi => oi.Quantity).IsRequired();
            entity.Property(oi => oi.Price).IsRequired();
        });
    }
    
}