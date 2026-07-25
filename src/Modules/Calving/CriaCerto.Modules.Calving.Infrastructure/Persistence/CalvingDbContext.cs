using CriaCerto.Modules.Calving.Application.Abstractions;
using CriaCerto.Modules.Calving.Application.Domain;
using Microsoft.EntityFrameworkCore;
using CalvingEntity = CriaCerto.Modules.Calving.Application.Domain.Calving;

namespace CriaCerto.Modules.Calving.Infrastructure.Persistence;

public sealed class CalvingDbContext : DbContext, ICalvingDbContext
{
    public CalvingDbContext(DbContextOptions<CalvingDbContext> options)
        : base(options)
    {
    }

    public DbSet<Calf> Calves => Set<Calf>();
    public DbSet<CalvingEntity> Calvings => Set<CalvingEntity>();
    public DbSet<Weaning> Weanings => Set<Weaning>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("calving");

        modelBuilder.Entity<Calf>(builder =>
        {
            builder.ToTable("Calves");
            builder.HasKey(c => c.Id);
            builder.HasIndex(c => c.TagId);
            builder.Property(c => c.TagId).HasMaxLength(50).IsRequired();
            builder.Property(c => c.Breed).HasMaxLength(100).IsRequired();
            builder.Property(c => c.Sex).HasMaxLength(5).IsRequired();
            builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(40);
        });

        modelBuilder.Entity<CalvingEntity>(builder =>
        {
            builder.ToTable("Calvings");
            builder.HasKey(c => c.Id);
            builder.HasIndex(c => c.MotherCowId);
            builder.Property(c => c.Type).HasConversion<string>().HasMaxLength(40);
            builder.Property(c => c.Condition).HasConversion<string>().HasMaxLength(40);
        });

        modelBuilder.Entity<Weaning>(builder =>
        {
            builder.ToTable("Weanings");
            builder.HasKey(w => w.Id);
            builder.HasIndex(w => w.CalfId);
            builder.Property(w => w.WeaningWeightKg).HasPrecision(8, 2);
            builder.Property(w => w.Adjusted205DayWeightKg).HasPrecision(8, 2);
        });
    }
}
