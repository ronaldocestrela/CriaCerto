using CriaCerto.Modules.Maternity.Application.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CriaCerto.Modules.Maternity.Infrastructure.Persistence.Configurations;

public sealed class FarrowingConfiguration : IEntityTypeConfiguration<Farrowing>
{
    public void Configure(EntityTypeBuilder<Farrowing> builder)
    {
        builder.ToTable("Farrowings", "Maternity");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id)
            .ValueGeneratedNever();

        builder.Property(f => f.SowId)
            .IsRequired();

        builder.Property(f => f.TenantId)
            .IsRequired();

        builder.Property(f => f.FarrowingDate)
            .IsRequired();

        builder.Property(f => f.LiveBorn)
            .IsRequired();

        builder.Property(f => f.Stillborn)
            .IsRequired();

        builder.Property(f => f.Mummified)
            .IsRequired();

        builder.Property(f => f.LitterWeightKg)
            .HasColumnType("decimal(6,2)")
            .IsRequired();

        builder.Property(f => f.MaternityRoomId)
            .HasMaxLength(60);

        builder.Property(f => f.Assisted)
            .IsRequired();

        builder.Property(f => f.Notes)
            .HasMaxLength(500);

        builder.Ignore(f => f.TotalBorn);
        builder.Ignore(f => f.AveragePigletWeightKg);
        builder.Ignore(f => f.DomainEvents);

        builder.HasIndex(f => f.TenantId);
        builder.HasIndex(f => f.SowId);
        builder.HasIndex(f => f.FarrowingDate);
    }
}
