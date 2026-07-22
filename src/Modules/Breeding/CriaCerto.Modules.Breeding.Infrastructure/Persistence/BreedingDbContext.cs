using CriaCerto.Modules.Breeding.Application.Abstractions;
using CriaCerto.Modules.Breeding.Application.Domain;
using Microsoft.EntityFrameworkCore;

namespace CriaCerto.Modules.Breeding.Infrastructure.Persistence;

public sealed class BreedingDbContext : DbContext, IBreedingDbContext
{
    public BreedingDbContext(DbContextOptions<BreedingDbContext> options)
        : base(options)
    {
    }

    public DbSet<Sow> Sows => Set<Sow>();
    public DbSet<Boar> Boars => Set<Boar>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("breeding");

        modelBuilder.Entity<Sow>(builder =>
        {
            builder.ToTable("Sows");
            builder.HasKey(s => s.Id);
            builder.HasIndex(s => s.TagId).IsUnique();
            builder.Property(s => s.TagId).HasMaxLength(40).IsRequired();
            builder.Property(s => s.Nickname).HasMaxLength(100);
            builder.Property(s => s.PbbRegistration).HasMaxLength(80);
            builder.Property(s => s.Breed).HasMaxLength(120).IsRequired();
            builder.Property(s => s.Origin).HasMaxLength(120);
            builder.Property(s => s.FatherTagId).HasMaxLength(40);
            builder.Property(s => s.MotherTagId).HasMaxLength(40);
            builder.Property(s => s.Location).HasMaxLength(120);
            builder.Property(s => s.LastEventName).HasMaxLength(120);
            builder.Property(s => s.EntryWeightKg).HasPrecision(8, 2);
            builder.Property(s => s.CurrentWeightKg).HasPrecision(8, 2);
            builder.Property(s => s.AverageDailyGain).HasPrecision(8, 3);
            builder.Property(s => s.BodyConditionScore).HasConversion<string>().HasMaxLength(30);
            builder.Property(s => s.ReproductiveStatus).HasConversion<string>().HasMaxLength(30);
            builder.Property(s => s.LifecycleStatus).HasConversion<string>().HasMaxLength(30);
            builder.Ignore(s => s.RequiresAttention);
            builder.Navigation(s => s.Events).HasField("_events").UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.OwnsMany(s => s.Events, eventsBuilder =>
            {
                eventsBuilder.ToTable("SowEvents");
                eventsBuilder.WithOwner().HasForeignKey("SowId");
                eventsBuilder.HasKey(e => e.Id);
                eventsBuilder.Property(e => e.EventType).HasMaxLength(80).IsRequired();
                eventsBuilder.Property(e => e.Title).HasMaxLength(160).IsRequired();
                eventsBuilder.Property(e => e.Notes).HasMaxLength(500);
            });
        });

        modelBuilder.Entity<Boar>(builder =>
        {
            builder.ToTable("Boars");
            builder.HasKey(b => b.Id);
            builder.HasIndex(b => b.TagId).IsUnique();
            builder.Property(b => b.TagId).HasMaxLength(40).IsRequired();
            builder.Property(b => b.Nickname).HasMaxLength(100);
            builder.Property(b => b.PbbRegistration).HasMaxLength(80);
            builder.Property(b => b.Breed).HasMaxLength(120).IsRequired();
            builder.Property(b => b.Origin).HasMaxLength(120);
            builder.Property(b => b.FatherTagId).HasMaxLength(40);
            builder.Property(b => b.MotherTagId).HasMaxLength(40);
            builder.Property(b => b.Location).HasMaxLength(120);
            builder.Property(b => b.LastEventName).HasMaxLength(120);
            builder.Property(b => b.EntryWeightKg).HasPrecision(8, 2);
            builder.Property(b => b.CurrentWeightKg).HasPrecision(8, 2);
            builder.Property(b => b.AverageDailyGain).HasPrecision(8, 3);
            builder.Property(b => b.BodyConditionScore).HasConversion<string>().HasMaxLength(30);
            builder.Property(b => b.LifecycleStatus).HasConversion<string>().HasMaxLength(30);
            builder.Ignore(b => b.RequiresAttention);
            builder.Navigation(b => b.Events).HasField("_events").UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.OwnsMany(b => b.Events, eventsBuilder =>
            {
                eventsBuilder.ToTable("BoarEvents");
                eventsBuilder.WithOwner().HasForeignKey("BoarId");
                eventsBuilder.HasKey(e => e.Id);
                eventsBuilder.Property(e => e.EventType).HasMaxLength(80).IsRequired();
                eventsBuilder.Property(e => e.Title).HasMaxLength(160).IsRequired();
                eventsBuilder.Property(e => e.Notes).HasMaxLength(500);
            });
        });
    }
}
