using CriaCerto.Modules.Maternity.Application.Domain;

namespace CriaCerto.Modules.Maternity.Application.Abstractions;

public interface IFarrowingRepository
{
    Task AddAsync(Farrowing farrowing, CancellationToken cancellationToken = default);
    Task<Farrowing?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Farrowing>> GetBySowIdAsync(Guid sowId, CancellationToken cancellationToken = default);
    Task<List<Farrowing>> GetByMaternityRoomAsync(string maternityRoomId, CancellationToken cancellationToken = default);
    Task<List<Farrowing>> GetAllAsync(CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
