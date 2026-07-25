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
    public DbSet<ProductionUnit> ProductionUnits => Set<ProductionUnit>();

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
            builder.Property(u => u.PhoneNumber).HasMaxLength(30);
            builder.Property(u => u.PasswordResetToken).HasMaxLength(100);
            builder.Property(u => u.PasswordResetTokenExpiresAt);
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
            builder.Property(t => t.City).HasMaxLength(100);
            builder.Property(t => t.StateRegistration).HasMaxLength(50);
            builder.Property(t => t.AreaInHectares).HasPrecision(18, 2);
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

        modelBuilder.Entity<ProductionUnit>(builder =>
        {
            builder.ToTable("ProductionUnits");
            builder.HasKey(pu => pu.Id);
            builder.Property(pu => pu.Code).HasMaxLength(50).IsRequired();
            builder.Property(pu => pu.Name).HasMaxLength(100).IsRequired();
            builder.Property(pu => pu.Type).HasMaxLength(50).IsRequired();
            builder.Property(pu => pu.Status).HasMaxLength(50).IsRequired();
            builder.Property(pu => pu.LocationDetails).HasMaxLength(250);

            builder.HasOne(pu => pu.Tenant)
                .WithMany(t => t.ProductionUnits)
                .HasForeignKey(pu => pu.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

