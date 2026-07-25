using CriaCerto.Modules.Maternity.Application.Abstractions;
using CriaCerto.Modules.Maternity.Application.Domain;
using CriaCerto.Modules.Maternity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CriaCerto.Modules.Maternity.Infrastructure.Repositories;

public sealed class FarrowingRepository : IFarrowingRepository
{
    private readonly MaternityDbContext _context;

    public FarrowingRepository(MaternityDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Farrowing farrowing, CancellationToken cancellationToken = default)
    {
        await _context.Farrowings.AddAsync(farrowing, cancellationToken);
    }

    public async Task<Farrowing?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Farrowings
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<List<Farrowing>> GetBySowIdAsync(Guid sowId, CancellationToken cancellationToken = default)
    {
        return await _context.Farrowings
            .Where(f => f.SowId == sowId)
            .OrderByDescending(f => f.FarrowingDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Farrowing>> GetByMaternityRoomAsync(string maternityRoomId, CancellationToken cancellationToken = default)
    {
        return await _context.Farrowings
            .Where(f => f.MaternityRoomId == maternityRoomId)
            .OrderByDescending(f => f.FarrowingDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Farrowing>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Farrowings
            .OrderByDescending(f => f.FarrowingDate)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
