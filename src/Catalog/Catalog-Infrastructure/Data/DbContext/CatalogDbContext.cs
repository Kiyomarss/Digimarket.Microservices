using Catalog_Domain.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Catalog_Infrastructure.Data.DbContext;

public class CatalogDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
        : base(options) { }
    
    public DbSet<Product> Product { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}