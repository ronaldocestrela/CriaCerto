using CriaCerto.Modules.Maternity.Application.Abstractions;
using CriaCerto.Modules.Maternity.Application.Domain;
using CriaCerto.Modules.Maternity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CriaCerto.Modules.Maternity.Infrastructure.Repositories;

public sealed class WeaningRepository : IWeaningRepository
{
    private readonly MaternityDbContext _context;

    public WeaningRepository(MaternityDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Weaning weaning, CancellationToken cancellationToken = default)
    {
        await _context.Weanings.AddAsync(weaning, cancellationToken);
    }

    public async Task<Weaning?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Weanings
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<Weaning?> GetByFarrowingIdAsync(Guid farrowingId, CancellationToken cancellationToken = default)
    {
        return await _context.Weanings
            .FirstOrDefaultAsync(w => w.FarrowingId == farrowingId, cancellationToken);
    }

    public async Task<List<Weaning>> GetBySowIdAsync(Guid sowId, CancellationToken cancellationToken = default)
    {
        return await _context.Weanings
            .Where(w => w.SowId == sowId)
            .OrderByDescending(w => w.WeaningDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Weaning>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Weanings
            .OrderByDescending(w => w.WeaningDate)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
