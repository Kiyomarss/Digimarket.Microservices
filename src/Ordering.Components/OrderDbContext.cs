using System.Text.Json;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordering.Components.StateMachines;

namespace Ordering.Components;

public class OrderDbContext : SagaDbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options)
        : base(options) { }

    protected override IEnumerable<ISagaClassMap> Configurations
    {
        get { yield return new OrderStateMap(); }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        MapRegistration(modelBuilder);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }

    static void MapRegistration(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderWrapper>(entity =>
        {
            entity.ToTable("orders");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Data)
                  .HasColumnName("data")
                  .HasColumnType("jsonb")
                  .HasConversion(
                                 v => JsonSerializer.Serialize(v, new JsonSerializerOptions()),
                                 v => JsonSerializer.Deserialize<Order>(v, new JsonSerializerOptions())!
                                );
        });
    }
}