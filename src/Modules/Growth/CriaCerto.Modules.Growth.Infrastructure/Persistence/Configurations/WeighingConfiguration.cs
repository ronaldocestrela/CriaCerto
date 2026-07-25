using CriaCerto.Modules.Growth.Application.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CriaCerto.Modules.Growth.Infrastructure.Persistence.Configurations;

public sealed class WeighingConfiguration : IEntityTypeConfiguration<Weighing>
{
    public void Configure(EntityTypeBuilder<Weighing> builder)
    {
        builder.ToTable("weighings");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.AnimalTagId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(w => w.WeightKg)
            .HasPrecision(8, 2);

        builder.Property(w => w.CarcassYieldPercentage)
            .HasPrecision(5, 2);

        builder.Property(w => w.CalculatedArrobasTotal)
            .HasPrecision(8, 2);

        builder.Property(w => w.CalculatedAdgKgPerDay)
            .HasPrecision(6, 2);

        builder.Property(w => w.CalculatedMonthlyArrobaGain)
            .HasPrecision(6, 2);

        builder.Property(w => w.Notes)
            .HasMaxLength(500);

        builder.HasIndex(w => new { w.TenantId, w.AnimalTagId, w.WeighingDate });
        builder.HasIndex(w => new { w.TenantId, w.LotId });
    }
}
