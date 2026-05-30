using Identity.Domain.Domain.Entities;
using MassTransit;

using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Data.DbContext;

public class IdentityDbContext : Microsoft.EntityFrameworkCore.DbContext

{

    public IdentityDbContext(DbContextOptions<IdentityDbContext> options)

        : base(options) { }

// Domain Entities

    public DbSet<User> Users { get; set; }

    public DbSet<Role> Roles { get; set; }

    public DbSet<UserRole> UserRoles { get; set; }

    public DbSet<UserClaim> UserClaims { get; set; }

    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)

    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);
        
        modelBuilder.UseOpenIddict();
        
        modelBuilder.AddInboxStateEntity();

        modelBuilder.AddOutboxMessageEntity();

        modelBuilder.AddOutboxStateEntity();
    }

}