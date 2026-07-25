using CriaCerto.Modules.Growth.Application.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CriaCerto.Modules.Growth.Infrastructure.Persistence.Configurations;

public sealed class LotConfiguration : IEntityTypeConfiguration<Lot>
{
    public void Configure(EntityTypeBuilder<Lot> builder)
    {
        builder.ToTable("Lots", "growth");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(l => l.Code)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(l => l.Category)
            .IsRequired();

        builder.Property(l => l.HeadCount)
            .IsRequired();

        builder.Property(l => l.AverageWeightKg)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(l => l.Status)
            .IsRequired();

        builder.Property(l => l.TenantId)
            .IsRequired();

        builder.HasIndex(l => new { l.TenantId, l.Code, l.Status });
    }
}
