using CriaCerto.BuildingBlocks.Abstractions.ReferenceData;
using Microsoft.EntityFrameworkCore;

namespace CriaCerto.BuildingBlocks.Application.Abstractions;

public interface IFoundationDbContext
{
    DbSet<BovineBreed> BovineBreeds { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
