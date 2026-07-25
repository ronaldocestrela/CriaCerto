using CriaCerto.Modules.Maternity.Application.Abstractions;
using CriaCerto.Modules.Maternity.Application.Domain;
using CriaCerto.Modules.Maternity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CriaCerto.Modules.Maternity.Infrastructure.Repositories;

public sealed class PigletTransferRepository : IPigletTransferRepository
{
    private readonly MaternityDbContext _context;

    public PigletTransferRepository(MaternityDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(PigletTransfer transfer, CancellationToken cancellationToken = default)
    {
        await _context.PigletTransfers.AddAsync(transfer, cancellationToken);
    }

    public async Task<PigletTransfer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.PigletTransfers
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<List<PigletTransfer>> GetBySourceFarrowingIdAsync(Guid farrowingId, CancellationToken cancellationToken = default)
    {
        return await _context.PigletTransfers
            .Where(t => t.SourceFarrowingId == farrowingId)
            .OrderByDescending(t => t.TransferDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<PigletTransfer>> GetByTargetFarrowingIdAsync(Guid farrowingId, CancellationToken cancellationToken = default)
    {
        return await _context.PigletTransfers
            .Where(t => t.TargetFarrowingId == farrowingId)
            .OrderByDescending(t => t.TransferDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<PigletTransfer>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.PigletTransfers
            .OrderByDescending(t => t.TransferDate)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
