using CriaCerto.Modules.Growth.Application.Abstractions;
using CriaCerto.Modules.Growth.Application.Domain;
using Microsoft.EntityFrameworkCore;

namespace CriaCerto.Modules.Growth.Infrastructure.Persistence;

public sealed class GrowthDbContext : DbContext, IGrowthDbContext
{
    public GrowthDbContext(DbContextOptions<GrowthDbContext> options)
        : base(options)
    {
    }

    public DbSet<PasturePaddock> Paddocks => Set<PasturePaddock>();
    public DbSet<Lot> Lots => Set<Lot>();
    public DbSet<LotMovement> LotMovements => Set<LotMovement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("growth");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GrowthDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
