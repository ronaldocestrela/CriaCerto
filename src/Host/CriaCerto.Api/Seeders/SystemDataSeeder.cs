using CriaCerto.BuildingBlocks.Infrastructure.Persistence;
using CriaCerto.BuildingBlocks.Infrastructure.Persistence.Seeders;
using CriaCerto.Modules.Sanitary.Infrastructure.Persistence;
using CriaCerto.Modules.Sanitary.Infrastructure.Persistence.Seeders;

namespace CriaCerto.Api.Seeders;

public static class SystemDataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger("SystemDataSeeder");

        try
        {
            var foundationDb = scope.ServiceProvider.GetRequiredService<FoundationDbContext>();
            var sanitaryDb = scope.ServiceProvider.GetRequiredService<SanitaryDbContext>();

            await SeedDataAsync(foundationDb, sanitaryDb, cancellationToken);
            logger?.LogInformation("[SystemDataSeeder] Reference data seeded successfully.");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "[SystemDataSeeder] Error seeding reference data.");
            throw;
        }
    }

    public static async Task SeedDataAsync(
        FoundationDbContext foundationDb,
        SanitaryDbContext sanitaryDb,
        CancellationToken cancellationToken = default)
    {
        await BovineBreedSeeder.SeedAsync(foundationDb, cancellationToken);
        await VaccineReferenceSeeder.SeedAsync(sanitaryDb, cancellationToken);
    }
}
