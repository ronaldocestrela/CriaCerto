using CriaCerto.BuildingBlocks.Abstractions.ReferenceData;
using CriaCerto.BuildingBlocks.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace CriaCerto.BuildingBlocks.Infrastructure.Persistence;

public sealed class FoundationDbContext : DbContext, IFoundationDbContext
{
    public DbSet<BovineBreed> BovineBreeds => Set<BovineBreed>();

    public FoundationDbContext(DbContextOptions<FoundationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("foundation");

        modelBuilder.Entity<BovineBreed>(builder =>
        {
            builder.ToTable("bovine_breeds");
            builder.HasKey(b => b.Id);
            builder.HasIndex(b => b.Code).IsUnique();
            builder.Property(b => b.Code).HasMaxLength(20).IsRequired();
            builder.Property(b => b.Name).HasMaxLength(100).IsRequired();
            builder.Property(b => b.Category).HasMaxLength(50);
            builder.Property(b => b.Aptitude).HasMaxLength(50);
            builder.Property(b => b.Origin).HasMaxLength(50);
        });
    }
}