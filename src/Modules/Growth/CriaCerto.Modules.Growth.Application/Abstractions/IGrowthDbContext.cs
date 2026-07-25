using CriaCerto.Modules.Growth.Application.Domain;
using Microsoft.EntityFrameworkCore;

namespace CriaCerto.Modules.Growth.Application.Abstractions;

public interface IGrowthDbContext
{
    DbSet<PasturePaddock> Paddocks { get; }
    DbSet<Lot> Lots { get; }
    DbSet<LotMovement> LotMovements { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
