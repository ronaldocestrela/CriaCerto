using CriaCerto.Modules.Breeding.Application.Domain;
using Microsoft.EntityFrameworkCore;

namespace CriaCerto.Modules.Breeding.Application.Abstractions;

public interface IBreedingDbContext
{
    DbSet<Sow> Sows { get; }
    DbSet<Boar> Boars { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
