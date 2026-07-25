using CriaCerto.Modules.Sanitary.Application.Contracts;
using CriaCerto.Modules.Sanitary.Application.Domain;
using Microsoft.EntityFrameworkCore;

namespace CriaCerto.Modules.Sanitary.Infrastructure.Persistence;

public class SanitaryDbContext : DbContext, ISanitaryDbContext
{
    public DbSet<VaccinationCampaign> VaccinationCampaigns => Set<VaccinationCampaign>();
    public DbSet<TreatmentRecord> TreatmentRecords => Set<TreatmentRecord>();

    public SanitaryDbContext(DbContextOptions<SanitaryDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("sanitary");

        modelBuilder.Entity<VaccinationCampaign>(builder =>
        {
            builder.ToTable("vaccination_campaigns");
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Name).HasMaxLength(150).IsRequired();
            builder.Property(c => c.Description).HasMaxLength(500);
        });

        modelBuilder.Entity<TreatmentRecord>(builder =>
        {
            builder.ToTable("treatment_records");
            builder.HasKey(t => t.Id);
            builder.Property(t => t.ProductCommercialName).HasMaxLength(150).IsRequired();
            builder.Property(t => t.BatchNumber).HasMaxLength(50);
            builder.Property(t => t.Dosage).HasMaxLength(50);
            builder.Property(t => t.AppliedByVeterinarian).HasMaxLength(150);
            builder.Property(t => t.Notes).HasMaxLength(500);
        });
    }
}
