using CriaCerto.Modules.Growth.Application.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CriaCerto.Modules.Growth.Infrastructure.Persistence.Configurations;

public sealed class PasturePaddockConfiguration : IEntityTypeConfiguration<PasturePaddock>
{
    public void Configure(EntityTypeBuilder<PasturePaddock> builder)
    {
        builder.ToTable("PasturePaddocks", "growth");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.Code)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(p => p.AreaHectares)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.MaxCapacityUA)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.Status)
            .IsRequired();

        builder.Property(p => p.TenantId)
            .IsRequired();

        builder.HasIndex(p => new { p.TenantId, p.Code }).IsUnique();
    }
}
