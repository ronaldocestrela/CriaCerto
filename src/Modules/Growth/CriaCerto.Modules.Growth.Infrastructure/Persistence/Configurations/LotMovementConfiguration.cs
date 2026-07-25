using CriaCerto.Modules.Growth.Application.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CriaCerto.Modules.Growth.Infrastructure.Persistence.Configurations;

public sealed class LotMovementConfiguration : IEntityTypeConfiguration<LotMovement>
{
    public void Configure(EntityTypeBuilder<LotMovement> builder)
    {
        builder.ToTable("LotMovements", "growth");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.LotId)
            .IsRequired();

        builder.Property(m => m.HeadCountMoved)
            .IsRequired();

        builder.Property(m => m.Notes)
            .HasMaxLength(500);

        builder.Property(m => m.TenantId)
            .IsRequired();

        builder.HasIndex(m => new { m.TenantId, m.LotId });
    }
}
