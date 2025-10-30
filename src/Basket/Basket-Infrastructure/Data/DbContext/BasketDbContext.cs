using Basket.Domain.Domain.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Basket.Infrastructure.Data.DbContext;

public class BasketDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public BasketDbContext(DbContextOptions<BasketDbContext> options)
        : base(options) { }
    
    public DbSet<BasketEntity> Basket { get; set; }
    public DbSet<BasketItem> BasketItem { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BasketDbContext).Assembly);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}