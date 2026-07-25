using CriaCerto.BuildingBlocks.Abstractions.Results;

namespace CriaCerto.Modules.Sanitary.Application.Domain;

public sealed class TreatmentRecord
{
    public Guid Id { get; private set; }
    public Guid? AnimalId { get; private set; }
    public Guid? LotId { get; private set; }
    public string ProductCommercialName { get; private set; } = string.Empty;
    public TreatmentType Type { get; private set; }
    public string? BatchNumber { get; private set; }
    public string Dosage { get; private set; } = string.Empty;
    public int WithdrawalDays { get; private set; }
    public DateTime ApplicationDateUtc { get; private set; }
    public DateTime WithdrawalEndDateUtc { get; private set; }
    public string? AppliedByVeterinarian { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private TreatmentRecord() { }

    public static Result<TreatmentRecord> Create(
        Guid? animalId,
        string productCommercialName,
        TreatmentType type,
        string? batchNumber,
        string dosage,
        int withdrawalDays,
        DateTime applicationDateUtc,
        string? appliedByVeterinarian = null,
        Guid? lotId = null,
        string? notes = null)
    {
        if (!animalId.HasValue && !lotId.HasValue)
            return Result.Failure<TreatmentRecord>(SanitaryErrors.EmptyAnimalOrLot);

        if (string.IsNullOrWhiteSpace(productCommercialName))
            return Result.Failure<TreatmentRecord>(Error.Validation("Sanitary.EmptyProductName", "O nome comercial do produto é obrigatório."));

        if (withdrawalDays < 0)
            return Result.Failure<TreatmentRecord>(SanitaryErrors.InvalidWithdrawalDays);

        var treatment = new TreatmentRecord
        {
            Id = Guid.NewGuid(),
            AnimalId = animalId,
            LotId = lotId,
            ProductCommercialName = productCommercialName.Trim(),
            Type = type,
            BatchNumber = batchNumber?.Trim(),
            Dosage = dosage.Trim(),
            WithdrawalDays = withdrawalDays,
            ApplicationDateUtc = applicationDateUtc,
            WithdrawalEndDateUtc = applicationDateUtc.AddDays(withdrawalDays),
            AppliedByVeterinarian = appliedByVeterinarian?.Trim(),
            Notes = notes?.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };

        return Result.Success(treatment);
    }

    public bool IsWithdrawalPeriodActive(DateTime checkDateUtc)
    {
        return checkDateUtc < WithdrawalEndDateUtc;
    }
}
