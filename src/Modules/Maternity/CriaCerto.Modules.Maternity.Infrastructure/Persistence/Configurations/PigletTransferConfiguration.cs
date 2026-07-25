using CriaCerto.Modules.Maternity.Application.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CriaCerto.Modules.Maternity.Infrastructure.Persistence.Configurations;

public sealed class PigletTransferConfiguration : IEntityTypeConfiguration<PigletTransfer>
{
    public void Configure(EntityTypeBuilder<PigletTransfer> builder)
    {
        builder.ToTable("PigletTransfers", "Maternity");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .ValueGeneratedNever();

        builder.Property(t => t.TenantId)
            .IsRequired();

        builder.Property(t => t.SourceFarrowingId)
            .IsRequired();

        builder.Property(t => t.SourceSowId)
            .IsRequired();

        builder.Property(t => t.TargetFarrowingId)
            .IsRequired();

        builder.Property(t => t.TargetSowId)
            .IsRequired();

        builder.Property(t => t.Quantity)
            .IsRequired();

        builder.Property(t => t.TransferDate)
            .IsRequired();

        builder.Property(t => t.Notes)
            .HasMaxLength(500);

        builder.Ignore(t => t.DomainEvents);

        builder.HasIndex(t => t.TenantId);
        builder.HasIndex(t => t.SourceFarrowingId);
        builder.HasIndex(t => t.TargetFarrowingId);
    }
}
