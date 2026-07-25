using CriaCerto.Modules.Breeding.Application.Domain;
using Microsoft.EntityFrameworkCore;

namespace CriaCerto.Modules.Breeding.Application.Abstractions;

public interface IBreedingDbContext
{
    DbSet<Cow> Cows { get; }
    DbSet<Bull> Bulls { get; }
    DbSet<SemenBatch> SemenBatches { get; }
    DbSet<IatfProtocol> IatfProtocols { get; }
    DbSet<PregnancyDiagnosis> PregnancyDiagnoses { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
