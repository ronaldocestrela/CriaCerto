using Microsoft.EntityFrameworkCore;

namespace CriaCerto.BuildingBlocks.Infrastructure.Persistence;

public sealed class FoundationDbContext : DbContext
{
    public FoundationDbContext(DbContextOptions<FoundationDbContext> options)
        : base(options)
    {
    }
}