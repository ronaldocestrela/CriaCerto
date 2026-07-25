using CriaCerto.BuildingBlocks.Abstractions.Results;
using CriaCerto.Modules.Growth.Application.Domain.Services;

namespace CriaCerto.Modules.Growth.Application.Domain;

public sealed class Weighing
{
    public const decimal DefaultCarcassYieldPercentage = 50.0m;
    public const decimal MinCarcassYieldPercentage = 40.0m;
    public const decimal MaxCarcassYieldPercentage = 65.0m;

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string AnimalTagId { get; private set; } = string.Empty;
    public Guid? LotId { get; private set; }
    public DateTime WeighingDate { get; private set; }
    public decimal WeightKg { get; private set; }
    public decimal CarcassYieldPercentage { get; private set; }
    public decimal CalculatedArrobasTotal { get; private set; }
    public decimal CalculatedAdgKgPerDay { get; private set; }
    public decimal CalculatedMonthlyArrobaGain { get; private set; }
    public bool IsWeightLossWarning { get; private set; }
    public string Notes { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; }

    private Weighing() { }

    public static Result<Weighing> Create(
        Guid tenantId,
        string animalTagId,
        Guid? lotId,
        DateTime weighingDate,
        decimal weightKg,
        decimal carcassYieldPercentage = DefaultCarcassYieldPercentage,
        string notes = "")
    {
        if (tenantId == Guid.Empty)
            return Result.Failure<Weighing>(Error.Validation("Weighing.InvalidTenant", "TenantId é obrigatório."));

        if (string.IsNullOrWhiteSpace(animalTagId))
            return Result.Failure<Weighing>(Error.Validation("Weighing.InvalidAnimalTag", "Identificação do animal (brinco/RFID) é obrigatória."));

        if (weightKg <= 0)
            return Result.Failure<Weighing>(Error.Validation("Weighing.InvalidWeight", "Peso em kg deve ser maior que zero."));

        if (carcassYieldPercentage < MinCarcassYieldPercentage || carcassYieldPercentage > MaxCarcassYieldPercentage)
            return Result.Failure<Weighing>(Error.Validation("Weighing.InvalidCarcassYield", $"Rendimento de carcaça deve estar entre {MinCarcassYieldPercentage}% e {MaxCarcassYieldPercentage}%."));

        if (weighingDate > DateTime.UtcNow.AddMinutes(5))
            return Result.Failure<Weighing>(Error.Validation("Weighing.FutureDate", "Data da pesagem não pode ser no futuro."));

        var arrobas = GrowthZootecnicCalculator.CalculateArrobas(weightKg, carcassYieldPercentage);

        var weighing = new Weighing
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AnimalTagId = animalTagId.Trim().ToUpperInvariant(),
            LotId = lotId,
            WeighingDate = weighingDate,
            WeightKg = weightKg,
            CarcassYieldPercentage = carcassYieldPercentage,
            CalculatedArrobasTotal = arrobas,
            CalculatedAdgKgPerDay = 0.0m,
            CalculatedMonthlyArrobaGain = 0.0m,
            IsWeightLossWarning = false,
            Notes = notes?.Trim() ?? string.Empty,
            CreatedAtUtc = DateTime.UtcNow
        };

        return Result.Success(weighing);
    }

    public void ApplyPreviousWeighing(Weighing previousWeighing)
    {
        if (previousWeighing is null || previousWeighing.WeighingDate >= WeighingDate)
            return;

        CalculatedAdgKgPerDay = GrowthZootecnicCalculator.CalculateAdg(WeightKg, previousWeighing.WeightKg, WeighingDate, previousWeighing.WeighingDate);
        CalculatedMonthlyArrobaGain = GrowthZootecnicCalculator.CalculateMonthlyArrobaGain(CalculatedAdgKgPerDay, CarcassYieldPercentage);
        IsWeightLossWarning = CalculatedAdgKgPerDay < 0.0m;
    }
}
