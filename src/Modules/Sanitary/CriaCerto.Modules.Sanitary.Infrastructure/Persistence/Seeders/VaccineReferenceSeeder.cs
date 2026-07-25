using CriaCerto.Modules.Sanitary.Application.Domain;
using Microsoft.EntityFrameworkCore;

namespace CriaCerto.Modules.Sanitary.Infrastructure.Persistence.Seeders;

public static class VaccineReferenceSeeder
{
    private static readonly List<VaccineReference> OfficialVaccines = new()
    {
        new(
            Guid.Parse("22222222-2222-2222-2222-222222222201"),
            "VAC-AFT",
            "Febre Aftosa",
            "Vacina Oleosa Bivalente",
            true,
            "Rebanho Geral conforme calendário oficial",
            recommendedAgeMonths: 3,
            boosterIntervalDays: 180,
            defaultWithdrawalDays: 0,
            notes: "Vacinação oficial conforme diretrizes estaduais do MAPA/PNEFA."
        ),
        new(
            Guid.Parse("22222222-2222-2222-2222-222222222202"),
            "VAC-BRU-B19",
            "Brucelose (Amostra B19)",
            "Vacina Viva Atenuada",
            true,
            "Fêmeas de 3 a 8 meses de idade",
            recommendedAgeMonths: 3,
            boosterIntervalDays: null,
            defaultWithdrawalDays: 0,
            notes: "Obrigatória para fêmeas bovinas de 3 a 8 meses. Exige marcação V no lado esquerdo da face."
        ),
        new(
            Guid.Parse("22222222-2222-2222-2222-222222222203"),
            "VAC-BRU-RB51",
            "Brucelose (Amostra RB51)",
            "Vacina Não Indutora de Sorologia",
            false,
            "Fêmeas acima de 8 meses não vacinadas com B19",
            recommendedAgeMonths: 8,
            boosterIntervalDays: null,
            defaultWithdrawalDays: 0,
            notes: "Alternativa sob prescrição e supervisão de médico veterinário habilitado."
        ),
        new(
            Guid.Parse("22222222-2222-2222-2222-222222222204"),
            "VAC-RAI",
            "Raiva dos Herbívoros",
            "Vacina Inativada",
            true,
            "Rebanho em regiões endêmicas",
            recommendedAgeMonths: 3,
            boosterIntervalDays: 30,
            defaultWithdrawalDays: 0,
            notes: "Obrigatória em regiões de controle de morcegos hematófagos Desmodus rotundus."
        ),
        new(
            Guid.Parse("22222222-2222-2222-2222-222222222205"),
            "VAC-CLO",
            "Clostridiose",
            "Vacina Polivalente (Carbúnculo Sintomático e Gangrena)",
            false,
            "Bezerros a partir de 2 meses e Rebanho Adulto",
            recommendedAgeMonths: 2,
            boosterIntervalDays: 30,
            defaultWithdrawalDays: 0,
            notes: "Recomendada imunização anual e reforço em bezerros desmamados."
        ),
        new(
            Guid.Parse("22222222-2222-2222-2222-222222222206"),
            "VAC-LEP",
            "Leptospirose",
            "Vacina Multivalente Inativada",
            false,
            "Matrizes e Reprodutores",
            recommendedAgeMonths: 6,
            boosterIntervalDays: 180,
            defaultWithdrawalDays: 0,
            notes: "Recomendada semestralmente para matrizes em reprodução."
        ),
        new(
            Guid.Parse("22222222-2222-2222-2222-222222222207"),
            "VAC-IBR-BVD",
            "IBR / BVD (Complexo Reprodutivo)",
            "Vacina Inativada Reprodutiva",
            false,
            "Fêmeas reprodutoras pré-estação de monta / IATF",
            recommendedAgeMonths: 12,
            boosterIntervalDays: 30,
            defaultWithdrawalDays: 0,
            notes: "Prevenção de mortalidade embrionária e abortos na IATF."
        )
    };

    public static async Task SeedAsync(SanitaryDbContext dbContext, CancellationToken cancellationToken = default)
    {
        var existingVaccineCodes = await dbContext.VaccineReferences
            .Select(v => v.Code)
            .ToListAsync(cancellationToken);

        var vaccinesToInsert = OfficialVaccines
            .Where(v => !existingVaccineCodes.Contains(v.Code))
            .ToList();

        if (vaccinesToInsert.Count > 0)
        {
            await dbContext.VaccineReferences.AddRangeAsync(vaccinesToInsert, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
