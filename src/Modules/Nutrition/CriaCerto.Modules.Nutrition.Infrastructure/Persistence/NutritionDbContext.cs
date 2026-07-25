using CriaCerto.Modules.Nutrition.Application.Domain;
using CriaCerto.Modules.Nutrition.Application.Features.SiloStockFeatures;
using Microsoft.EntityFrameworkCore;

namespace CriaCerto.Modules.Nutrition.Infrastructure.Persistence;

public sealed class NutritionDbContext : DbContext, INutritionDbContext
{
    public NutritionDbContext(DbContextOptions<NutritionDbContext> options)
        : base(options)
    {
    }

    public DbSet<SiloStock> SiloStocks => Set<SiloStock>();
    public DbSet<FeedRation> FeedRations => Set<FeedRation>();
    public DbSet<PastureSupplementation> PastureSupplementations => Set<PastureSupplementation>();
    public DbSet<DailyFeedBatch> DailyFeedBatches => Set<DailyFeedBatch>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("nutrition");

        modelBuilder.Entity<SiloStock>(builder =>
        {
            builder.ToTable("SiloStocks");
            builder.HasKey(s => s.Id);
            builder.Property(s => s.Name).HasMaxLength(150).IsRequired();
            builder.Property(s => s.UnitCostPerKg).HasPrecision(18, 4);
            builder.Property(s => s.CurrentStockKg).HasPrecision(18, 2);
            builder.Property(s => s.DryMatterPercentage).HasPrecision(5, 2);
            builder.Property(s => s.MinimumThresholdKg).HasPrecision(18, 2);
        });

        modelBuilder.Entity<FeedRation>(builder =>
        {
            builder.ToTable("FeedRations");
            builder.HasKey(r => r.Id);
            builder.Property(r => r.Name).HasMaxLength(150).IsRequired();
            builder.Property(r => r.DryMatterPercentage).HasPrecision(5, 2);
            builder.Property(r => r.CalculatedCostPerKg).HasPrecision(18, 4);

            builder.HasMany(r => r.Items)
                .WithOne()
                .HasForeignKey(i => i.FeedRationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FeedRationItem>(builder =>
        {
            builder.ToTable("FeedRationItems");
            builder.HasKey(i => i.Id);
            builder.Property(i => i.FeedItemName).HasMaxLength(150).IsRequired();
            builder.Property(i => i.Percentage).HasPrecision(5, 2);
            builder.Property(i => i.UnitCostPerKg).HasPrecision(18, 4);
        });

        modelBuilder.Entity<PastureSupplementation>(builder =>
        {
            builder.ToTable("PastureSupplementations");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.QuantityKg).HasPrecision(18, 2);
            builder.Property(p => p.CalculatedIntakeGramsPerHead).HasPrecision(18, 2);
        });

        modelBuilder.Entity<DailyFeedBatch>(builder =>
        {
            builder.ToTable("DailyFeedBatches");
            builder.HasKey(b => b.Id);
            builder.Property(b => b.OfferedAsFedKg).HasPrecision(18, 2);
            builder.Property(b => b.OfferedDryMatterKg).HasPrecision(18, 2);
        });

        base.OnModelCreating(modelBuilder);
    }
}
