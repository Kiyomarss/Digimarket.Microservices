using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Basket.Components;

public class BasketDbContext : DbContext
{
    public BasketDbContext(DbContextOptions<BasketDbContext> options)
        : base(options) { }
    
    public DbSet<Basket> Basket { get; set; }
    public DbSet<BasketItem> BasketItem { get; set; }
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
          modelBuilder.Entity<Basket>(entity =>
          {
                entity.ToTable("baskets");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.UserId).IsRequired();

                entity.HasMany(x => x.Items)
                      .WithOne(i => i.Basket)
                      .HasForeignKey(i => i.BasketId)
                      .OnDelete(DeleteBehavior.Cascade);
          });

          modelBuilder.Entity<BasketItem>(entity =>
          {
                entity.ToTable("basket_items");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.CatalogId).IsRequired();
                entity.Property(x => x.Quantity).IsRequired();
          });
    }
}