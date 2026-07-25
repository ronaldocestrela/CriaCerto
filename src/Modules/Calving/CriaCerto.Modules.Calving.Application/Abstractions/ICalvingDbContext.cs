using CriaCerto.Modules.Calving.Application.Domain;
using Microsoft.EntityFrameworkCore;

namespace CriaCerto.Modules.Calving.Application.Abstractions;

public interface ICalvingDbContext
{
    DbSet<Calf> Calves { get; }
    DbSet<Domain.Calving> Calvings { get; }
    DbSet<Weaning> Weanings { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
