using CriaCerto.Modules.Maternity.Application.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CriaCerto.Modules.Maternity.Infrastructure.Persistence.Configurations;

public sealed class WeaningConfiguration : IEntityTypeConfiguration<Weaning>
{
    public void Configure(EntityTypeBuilder<Weaning> builder)
    {
        builder.ToTable("Weanings", "Maternity");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Id)
            .ValueGeneratedNever();

        builder.Property(w => w.TenantId)
            .IsRequired();

        builder.Property(w => w.FarrowingId)
            .IsRequired();

        builder.Property(w => w.SowId)
            .IsRequired();

        builder.Property(w => w.WeaningDate)
            .IsRequired();

        builder.Property(w => w.WeanedCount)
            .IsRequired();

        builder.Property(w => w.TotalWeanedWeightKg)
            .HasColumnType("decimal(6,2)")
            .IsRequired();

        builder.Property(w => w.DestinationPenOrBatch)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(w => w.Notes)
            .HasMaxLength(500);

        builder.Ignore(w => w.AverageWeanedWeightKg);
        builder.Ignore(w => w.DomainEvents);

        builder.HasIndex(w => w.TenantId);
        builder.HasIndex(w => w.FarrowingId);
        builder.HasIndex(w => w.SowId);
    }
}
