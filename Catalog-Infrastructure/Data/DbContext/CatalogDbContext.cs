using Catalog.Components;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Catalog_Infrastructure.Data.DbContext;

public class CatalogDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
        : base(options) { }
    
    public DbSet<CatalogItem> CatalogItem { get; set; }
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
        modelBuilder.Entity<CatalogItem>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.AttributesJson).HasColumnType("jsonb");
        });

    }
}