using CriaCerto.Modules.Maternity.Application.Domain;

namespace CriaCerto.Modules.Maternity.Application.Abstractions;

public interface IPigletTransferRepository
{
    Task AddAsync(PigletTransfer transfer, CancellationToken cancellationToken = default);
    Task<PigletTransfer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<PigletTransfer>> GetBySourceFarrowingIdAsync(Guid farrowingId, CancellationToken cancellationToken = default);
    Task<List<PigletTransfer>> GetByTargetFarrowingIdAsync(Guid farrowingId, CancellationToken cancellationToken = default);
    Task<List<PigletTransfer>> GetAllAsync(CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
