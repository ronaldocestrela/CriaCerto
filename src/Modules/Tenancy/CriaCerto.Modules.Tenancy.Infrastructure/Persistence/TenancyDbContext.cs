using CriaCerto.Modules.Tenancy.Application.Abstractions;
using CriaCerto.Modules.Tenancy.Application.Domain;
using Microsoft.EntityFrameworkCore;

namespace CriaCerto.Modules.Tenancy.Infrastructure.Persistence;

public sealed class TenancyDbContext : DbContext, ITenancyDbContext
{
    public TenancyDbContext(DbContextOptions<TenancyDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<UserTenant> UserTenants => Set<UserTenant>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("tenancy");

        modelBuilder.Entity<User>(builder =>
        {
            builder.ToTable("Users");
            builder.HasKey(u => u.Id);
            builder.HasIndex(u => u.Email).IsUnique();
            builder.Property(u => u.Email).HasMaxLength(150).IsRequired();
            builder.Property(u => u.FullName).HasMaxLength(150).IsRequired();
            builder.Property(u => u.PasswordHash).HasMaxLength(255).IsRequired();
        });

        modelBuilder.Entity<Tenant>(builder =>
        {
            builder.ToTable("Tenants");
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Name).HasMaxLength(150).IsRequired();
            builder.Property(t => t.CNPJ).HasMaxLength(20).IsRequired();
            builder.Property(t => t.Status).HasMaxLength(50).IsRequired();
            builder.Property(t => t.SubscribedPlan).HasMaxLength(50).IsRequired();
            builder.Property(t => t.State).HasMaxLength(50);
            builder.Property(t => t.Type).HasMaxLength(100);
        });

        modelBuilder.Entity<UserTenant>(builder =>
        {
            builder.ToTable("UserTenants");
            builder.HasKey(ut => new { ut.UserId, ut.TenantId });

            builder.HasOne(ut => ut.User)
                .WithMany(u => u.UserTenants)
                .HasForeignKey(ut => ut.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ut => ut.Tenant)
                .WithMany(t => t.UserTenants)
                .HasForeignKey(ut => ut.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
