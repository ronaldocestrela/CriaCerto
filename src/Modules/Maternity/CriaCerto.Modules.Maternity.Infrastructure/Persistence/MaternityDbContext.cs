using CriaCerto.BuildingBlocks.Abstractions.Tenancy;
using CriaCerto.Modules.Maternity.Application.Domain;
using Microsoft.EntityFrameworkCore;

namespace CriaCerto.Modules.Maternity.Infrastructure.Persistence;

public sealed class MaternityDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    public MaternityDbContext(DbContextOptions<MaternityDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<Farrowing> Farrowings => Set<Farrowing>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("Maternity");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MaternityDbContext).Assembly);

        modelBuilder.Entity<Farrowing>()
            .HasQueryFilter(f => f.TenantId == _tenantContext.TenantId);

        base.OnModelCreating(modelBuilder);
    }
}
