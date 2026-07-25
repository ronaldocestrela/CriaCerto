using CriaCerto.Modules.Maternity.Application.Domain;

namespace CriaCerto.Modules.Maternity.Application.Abstractions;

public interface IWeaningRepository
{
    Task AddAsync(Weaning weaning, CancellationToken cancellationToken = default);
    Task<Weaning?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Weaning?> GetByFarrowingIdAsync(Guid farrowingId, CancellationToken cancellationToken = default);
    Task<List<Weaning>> GetBySowIdAsync(Guid sowId, CancellationToken cancellationToken = default);
    Task<List<Weaning>> GetAllAsync(CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
