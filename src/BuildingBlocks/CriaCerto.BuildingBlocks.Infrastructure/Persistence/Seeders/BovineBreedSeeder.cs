using CriaCerto.BuildingBlocks.Abstractions.ReferenceData;
using Microsoft.EntityFrameworkCore;

namespace CriaCerto.BuildingBlocks.Infrastructure.Persistence.Seeders;

public static class BovineBreedSeeder
{
    private static readonly List<BovineBreed> OfficialBreeds = new()
    {
        new(Guid.Parse("11111111-1111-1111-1111-111111111101"), "NEL", "Nelore", "Zebuíno", "Corte", "Brasil / Índia"),
        new(Guid.Parse("11111111-1111-1111-1111-111111111102"), "ANG", "Angus", "Taurino", "Corte", "Escócia"),
        new(Guid.Parse("11111111-1111-1111-1111-111111111103"), "BRA", "Brahman", "Zebuíno", "Corte", "EUA / Índia"),
        new(Guid.Parse("11111111-1111-1111-1111-111111111104"), "SEN", "Senepol", "Taurino", "Corte", "Ilha de Saint Croix"),
        new(Guid.Parse("11111111-1111-1111-1111-111111111105"), "GIR", "Gir", "Zebuíno", "Dupla Aptidão", "Índia"),
        new(Guid.Parse("11111111-1111-1111-1111-111111111106"), "GIRL", "Girolando", "Misto", "Leite", "Brasil"),
        new(Guid.Parse("11111111-1111-1111-1111-111111111107"), "TAB", "Tabapuã", "Zebuíno", "Corte", "Brasil"),
        new(Guid.Parse("11111111-1111-1111-1111-111111111108"), "CAN", "Canchim", "Sintético", "Corte", "Brasil"),
        new(Guid.Parse("11111111-1111-1111-1111-111111111109"), "BRG", "Brangus", "Sintético", "Corte", "EUA"),
        new(Guid.Parse("11111111-1111-1111-1111-111111111110"), "BRF", "Braford", "Sintético", "Corte", "EUA"),
        new(Guid.Parse("11111111-1111-1111-1111-111111111111"), "CRC", "Caracu", "Taurino Adaptado", "Dupla Aptidão", "Brasil"),
        new(Guid.Parse("11111111-1111-1111-1111-111111111112"), "GUZ", "Guzerá", "Zebuíno", "Dupla Aptidão", "Índia")
    };

    public static async Task SeedAsync(FoundationDbContext dbContext, CancellationToken cancellationToken = default)
    {
        var existingBreedCodes = await dbContext.BovineBreeds
            .Select(b => b.Code)
            .ToListAsync(cancellationToken);

        var breedsToInsert = OfficialBreeds
            .Where(b => !existingBreedCodes.Contains(b.Code))
            .ToList();

        if (breedsToInsert.Count > 0)
        {
            await dbContext.BovineBreeds.AddRangeAsync(breedsToInsert, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
